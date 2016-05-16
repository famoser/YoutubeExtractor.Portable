// ****************************************************************************
//
// FLV Extract
// Copyright (C) 2006-2012  J.D. Purcell (moitah@yahoo.com)
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//
// ****************************************************************************

using System.Collections.Generic;
using System.IO;
using Famoser.YoutubeExtractor.Portable.Helpers;

namespace Famoser.YoutubeExtractor.Portable.Extractors
{
    internal class Mp3AudioExtractor : IAudioExtractor
    {
        private readonly List<byte[]> _chunkBuffer;
        private readonly Stream _fileStream;
        private readonly List<uint> _frameOffsets;
        private readonly List<string> _warnings;
        private int _channelMode;
        private bool _delayWrite;
        private int _firstBitRate;
        private uint _firstFrameHeader;
        private bool _hasVbrHeader;
        private bool _isVbr;
        private int _mpegVersion;
        private int _sampleRate;
        private uint _totalFrameLength;
        private bool _writeVbrHeader;

        public Mp3AudioExtractor()
        {
            _fileStream = new MemoryStream();
            _warnings = new List<string>();
            _chunkBuffer = new List<byte[]>();
            _frameOffsets = new List<uint>();
            _delayWrite = true;
        }

        public Stream Save()
        {
            Flush();

            if (_writeVbrHeader)
            {
                _fileStream.Seek(0, SeekOrigin.Begin);
                WriteVbrHeader(false);
            }

            return _fileStream;
        }

        public void WriteChunk(byte[] chunk)
        {
            _chunkBuffer.Add(chunk);
            ParseMp3Frames(chunk);

            if (_delayWrite && _totalFrameLength >= 65536)
            {
                _delayWrite = false;
            }

            if (!_delayWrite)
            {
                Flush();
            }
        }

        private static int GetFrameDataOffset(int mpegVersion, int channelMode)
        {
            return 4 + (mpegVersion == 3 ?
                (channelMode == 3 ? 17 : 32) :
                (channelMode == 3 ? 9 : 17));
        }

        private static int GetFrameLength(int mpegVersion, int bitRate, int sampleRate, int padding)
        {
            return (mpegVersion == 3 ? 144 : 72) * bitRate / sampleRate + padding;
        }

        private void Flush()
        {
            foreach (byte[] chunk in _chunkBuffer)
            {
                _fileStream.Write(chunk, 0, chunk.Length);
            }

            _chunkBuffer.Clear();
        }

        private void ParseMp3Frames(byte[] buffer)
        {
            var mpeg1BitRate = new[] { 0, 32, 40, 48, 56, 64, 80, 96, 112, 128, 160, 192, 224, 256, 320, 0 };
            var mpeg2XBitRate = new[] { 0, 8, 16, 24, 32, 40, 48, 56, 64, 80, 96, 112, 128, 144, 160, 0 };
            var mpeg1SampleRate = new[] { 44100, 48000, 32000, 0 };
            var mpeg20SampleRate = new[] { 22050, 24000, 16000, 0 };
            var mpeg25SampleRate = new[] { 11025, 12000, 8000, 0 };

            int offset = 0;
            int length = buffer.Length;

            while (length >= 4)
            {
                int mpegVersion, sampleRate, channelMode;

                ulong header = (ulong)BigEndianBitConverter.ToUInt32(buffer, offset) << 32;

                if (BitHelper.Read(ref header, 11) != 0x7FF)
                {
                    break;
                }

                mpegVersion = BitHelper.Read(ref header, 2);
                int layer = BitHelper.Read(ref header, 2);
                BitHelper.Read(ref header, 1);
                int bitRate = BitHelper.Read(ref header, 4);
                sampleRate = BitHelper.Read(ref header, 2);
                int padding = BitHelper.Read(ref header, 1);
                BitHelper.Read(ref header, 1);
                channelMode = BitHelper.Read(ref header, 2);

                if (mpegVersion == 1 || layer != 1 || bitRate == 0 || bitRate == 15 || sampleRate == 3)
                {
                    break;
                }

                bitRate = (mpegVersion == 3 ? mpeg1BitRate[bitRate] : mpeg2XBitRate[bitRate]) * 1000;

                switch (mpegVersion)
                {
                    case 2:
                        sampleRate = mpeg20SampleRate[sampleRate];
                        break;

                    case 3:
                        sampleRate = mpeg1SampleRate[sampleRate];
                        break;

                    default:
                        sampleRate = mpeg25SampleRate[sampleRate];
                        break;
                }

                int frameLenght = GetFrameLength(mpegVersion, bitRate, sampleRate, padding);

                if (frameLenght > length)
                {
                    break;
                }

                bool isVbrHeaderFrame = false;

                if (_frameOffsets.Count == 0)
                {
                    // Check for an existing VBR header just to be safe (I haven't seen any in FLVs)
                    int o = offset + GetFrameDataOffset(mpegVersion, channelMode);

                    if (BigEndianBitConverter.ToUInt32(buffer, o) == 0x58696E67)
                    {
                        // "Xing"
                        isVbrHeaderFrame = true;
                        _delayWrite = false;
                        _hasVbrHeader = true;
                    }
                }

                if (!isVbrHeaderFrame)
                {
                    if (_firstBitRate == 0)
                    {
                        _firstBitRate = bitRate;
                        _mpegVersion = mpegVersion;
                        _sampleRate = sampleRate;
                        _channelMode = channelMode;
                        _firstFrameHeader = BigEndianBitConverter.ToUInt32(buffer, offset);
                    }

                    else if (!_isVbr && bitRate != _firstBitRate)
                    {
                        _isVbr = true;

                        if (!_hasVbrHeader)
                        {
                            if (_delayWrite)
                            {
                                WriteVbrHeader(true);
                                _writeVbrHeader = true;
                                _delayWrite = false;
                            }

                            else
                            {
                                _warnings.Add("Detected VBR too late, cannot add VBR header.");
                            }
                        }
                    }
                }

                _frameOffsets.Add(_totalFrameLength + (uint)offset);

                offset += frameLenght;
                length -= frameLenght;
            }

            _totalFrameLength += (uint)buffer.Length;
        }

        private void WriteVbrHeader(bool isPlaceholder)
        {
            var buffer = new byte[GetFrameLength(_mpegVersion, 64000, _sampleRate, 0)];

            if (!isPlaceholder)
            {
                uint header = _firstFrameHeader;
                int dataOffset = GetFrameDataOffset(_mpegVersion, _channelMode);
                header &= 0xFFFE0DFF; // Clear CRC, bitrate, and padding fields
                header |= (uint)(_mpegVersion == 3 ? 5 : 8) << 12; // 64 kbit/sec
                BitHelper.CopyBytes(buffer, 0, BigEndianBitConverter.GetBytes(header));
                BitHelper.CopyBytes(buffer, dataOffset, BigEndianBitConverter.GetBytes(0x58696E67)); // "Xing"
                BitHelper.CopyBytes(buffer, dataOffset + 4, BigEndianBitConverter.GetBytes((uint)0x7)); // Flags
                BitHelper.CopyBytes(buffer, dataOffset + 8, BigEndianBitConverter.GetBytes((uint)_frameOffsets.Count)); // Frame count
                BitHelper.CopyBytes(buffer, dataOffset + 12, BigEndianBitConverter.GetBytes(_totalFrameLength)); // File length

                for (int i = 0; i < 100; i++)
                {
                    int frameIndex = (int)((i / 100.0) * _frameOffsets.Count);

                    buffer[dataOffset + 16 + i] = (byte)(_frameOffsets[frameIndex] / (double)_totalFrameLength * 256.0);
                }
            }

            _fileStream.Write(buffer, 0, buffer.Length);
        }
    }
}
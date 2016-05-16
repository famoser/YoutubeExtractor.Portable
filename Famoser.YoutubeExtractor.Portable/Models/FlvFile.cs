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

using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Famoser.YoutubeExtractor.Portable.Exceptions;
using Famoser.YoutubeExtractor.Portable.Extractors;
using Famoser.YoutubeExtractor.Portable.Helpers;

namespace Famoser.YoutubeExtractor.Portable.Models
{
    internal class FlvFile
    {
        private readonly long _fileLength;
        private IAudioExtractor _audioExtractor;
        private long _fileOffset;
        private Stream _stream;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlvFile"/> class.
        /// </summary>
        /// <param name="stream">The content of the file.</param>
        public FlvFile(Stream stream)
        {
            _stream = stream;
            _fileOffset = 0;
            _fileLength = stream.Length;
        }

        public event ProgressChangedEventHandler ConversionProgressChanged;

        public bool ExtractedAudio { get; private set; }

        /// <exception cref="AudioExtractionException">The input file is not an FLV file.</exception>
        public async Task<Stream> ExtractStreams()
        {
            return await Task.Run(() =>
            {
                Seek(0);

                if (ReadUInt32() != 0x464C5601)
                {
                    // not a FLV file
                    throw new AudioExtractionException("Invalid input file. Impossible to extract audio track.");
                }

                ReadUInt8();
                uint dataOffset = ReadUInt32();

                Seek(dataOffset);

                ReadUInt32();

                while (_fileOffset < _fileLength)
                {
                    if (!ReadTag())
                    {
                        break;
                    }

                    if (_fileLength - _fileOffset < 4)
                    {
                        break;
                    }

                    ReadUInt32();

                    double progress = (_fileOffset*1.0/_fileLength)*100;

                    ConversionProgressChanged?.Invoke(this, new ProgressChangedEventArgs((int) progress, Guid.NewGuid()));
                }

                return _audioExtractor.Save();
            });
        }

        private IAudioExtractor GetAudioWriter(uint mediaInfo)
        {
            uint format = mediaInfo >> 4;

            switch (format)
            {
                case 14:
                case 2:
                    return new Mp3AudioExtractor();

                case 10:
                    return new AacAudioExtractor();
            }

            string typeStr;

            switch (format)
            {
                case 1:
                    typeStr = "ADPCM";
                    break;

                case 6:
                case 5:
                case 4:
                    typeStr = "Nellymoser";
                    break;

                default:
                    typeStr = "format=" + format;
                    break;
            }

            throw new AudioExtractionException("Unable to extract audio (" + typeStr + " is unsupported).");
        }

        private byte[] ReadBytes(int length)
        {
            var buff = new byte[length];

            _stream.Read(buff, 0, length);
            _fileOffset += length;

            return buff;
        }

        private bool ReadTag()
        {
            if (_fileLength - _fileOffset < 11)
                return false;

            // Read tag header
            uint tagType = ReadUInt8();
            uint dataSize = ReadUInt24();
            uint timeStamp = ReadUInt24();
            timeStamp |= ReadUInt8() << 24;
            ReadUInt24();

            // Read tag data
            if (dataSize == 0)
                return true;

            if (_fileLength - _fileOffset < dataSize)
                return false;

            uint mediaInfo = ReadUInt8();
            dataSize -= 1;
            byte[] data = ReadBytes((int)dataSize);

            if (tagType == 0x8)
            {
                // If we have no audio writer, create one
                if (_audioExtractor == null)
                {
                    _audioExtractor = GetAudioWriter(mediaInfo);
                    ExtractedAudio = _audioExtractor != null;
                }

                if (_audioExtractor == null)
                {
                    throw new InvalidOperationException("No supported audio writer found.");
                }

                _audioExtractor.WriteChunk(data);
            }
            return true;
        }

        private uint ReadUInt24()
        {
            var x = new byte[4];

            _stream.Read(x, 1, 3);
            _fileOffset += 3;

            return BigEndianBitConverter.ToUInt32(x, 0);
        }

        private uint ReadUInt32()
        {
            var x = new byte[4];

            _stream.Read(x, 0, 4);
            _fileOffset += 4;

            return BigEndianBitConverter.ToUInt32(x, 0);
        }

        private uint ReadUInt8()
        {
            _fileOffset += 1;
            return (uint)_stream.ReadByte();
        }

        private void Seek(long offset)
        {
            _stream.Seek(offset, SeekOrigin.Begin);
            _fileOffset = offset;
        }
    }
}
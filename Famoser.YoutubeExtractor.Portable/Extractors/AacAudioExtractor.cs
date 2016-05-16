﻿using System.IO;
using Famoser.YoutubeExtractor.Portable.Exceptions;
using Famoser.YoutubeExtractor.Portable.Helpers;

namespace Famoser.YoutubeExtractor.Portable.Extractors
{
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

    internal class AacAudioExtractor : IAudioExtractor
    {
        private readonly Stream _fileStream;
        private int _aacProfile;
        private int _channelConfig;
        private int _sampleRateIndex;

        public AacAudioExtractor()
        {
            _fileStream = new MemoryStream();
        }

        public void WriteChunk(byte[] chunk)
        {
            if (chunk.Length < 1)
            {
                return;
            }

            if (chunk[0] == 0)
            {
                // Header
                if (chunk.Length < 3)
                {
                    return;
                }

                ulong bits = (ulong)BigEndianBitConverter.ToUInt16(chunk, 1) << 48;

                _aacProfile = BitHelper.Read(ref bits, 5) - 1;
                _sampleRateIndex = BitHelper.Read(ref bits, 4);
                _channelConfig = BitHelper.Read(ref bits, 4);

                if (_aacProfile < 0 || _aacProfile > 3)
                    throw new AudioExtractionException("Unsupported AAC profile.");
                if (_sampleRateIndex > 12)
                    throw new AudioExtractionException("Invalid AAC sample rate index.");
                if (_channelConfig > 6)
                    throw new AudioExtractionException("Invalid AAC channel configuration.");
            }

            else
            {
                // Audio data
                int dataSize = chunk.Length - 1;
                ulong bits = 0;

                // Reference: WriteADTSHeader from FAAC's bitstream.c

                BitHelper.Write(ref bits, 12, 0xFFF);
                BitHelper.Write(ref bits, 1, 0);
                BitHelper.Write(ref bits, 2, 0);
                BitHelper.Write(ref bits, 1, 1);
                BitHelper.Write(ref bits, 2, _aacProfile);
                BitHelper.Write(ref bits, 4, _sampleRateIndex);
                BitHelper.Write(ref bits, 1, 0);
                BitHelper.Write(ref bits, 3, _channelConfig);
                BitHelper.Write(ref bits, 1, 0);
                BitHelper.Write(ref bits, 1, 0);
                BitHelper.Write(ref bits, 1, 0);
                BitHelper.Write(ref bits, 1, 0);
                BitHelper.Write(ref bits, 13, 7 + dataSize);
                BitHelper.Write(ref bits, 11, 0x7FF);
                BitHelper.Write(ref bits, 2, 0);

                _fileStream.Write(BigEndianBitConverter.GetBytes(bits), 1, 7);
                _fileStream.Write(chunk, 1, dataSize);
            }
        }

        public Stream Save()
        {
            return _fileStream;
        }
    }
}

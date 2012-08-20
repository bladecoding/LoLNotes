/*
	FluorineFx open source library 
	Copyright (C) 2007 Zoltan Csibi, zoltan@TheSilentGroup.com, FluorineFx.com 
	
	This library is free software; you can redistribute it and/or
	modify it under the terms of the GNU Lesser General Public
	License as published by the Free Software Foundation; either
	version 2.1 of the License, or (at your option) any later version.
	
	This library is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
	Lesser General Public License for more details.
	
	You should have received a copy of the GNU Lesser General Public
	License along with this library; if not, write to the Free Software
	Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*/
using System;
using System.Collections;
using System.IO;

namespace FluorineFx.IO.Mp3
{
    /// <summary>
    /// Header of an Mp3 frame.
    /// http://mpgedit.org/mpgedit/mpeg_format/mpeghdr.htm
    /// </summary>
    class Mp3Header
    {
        /// <summary>
        /// Sampling rates in hertz: 1. index = MPEG Version ID, 2. index = sampling rate index
        /// </summary>
        static int[,] SamplingRates = new int[,]
            { 
	            {11025, 12000, 8000,  },	// MPEG 2.5
	            {0,     0,     0,     },	// reserved
	            {22050, 24000, 16000, },	// MPEG 2
	            {44100, 48000, 32000  }		// MPEG 1
            };
        /// <summary>
        /// Bitrates: 1. index = LSF, 2. index = Layer, 3. index = bitrate index
        /// </summary>
        static int[,,] Bitrates = new int[,,]
            {
	            {	// MPEG 1
		            {0,32,64,96,128,160,192,224,256,288,320,352,384,416,448,},	// Layer1
		            {0,32,48,56, 64, 80, 96,112,128,160,192,224,256,320,384,},	// Layer2
		            {0,32,40,48, 56, 64, 80, 96,112,128,160,192,224,256,320,}	// Layer3
	            },
	            {	// MPEG 2, 2.5		
		            {0,32,48,56,64,80,96,112,128,144,160,176,192,224,256,},		// Layer1
		            {0,8,16,24,32,40,48,56,64,80,96,112,128,144,160,},			// Layer2
		            {0,8,16,24,32,40,48,56,64,80,96,112,128,144,160,}			// Layer3
	            }
            };
        /// <summary>
        /// Allowed combination of bitrate (1.index) and mono (2.index)
        /// </summary>
        static bool[,] AllowedModes = new bool[,]
            {
	            // {stereo, intensity stereo, dual channel allowed, single channel allowed}
	            {true,true},		// free mode
	            {false,true},		// 32
	            {false,true},		// 48
	            {false,true},		// 56
	            {true,true},		// 64
	            {false,true},		// 80
	            {true,true},		// 96
	            {true,true},		// 112
	            {true,true},		// 128
	            {true,true},		// 160
	            {true,true},		// 192
	            {true,false},		// 224
	            {true,false},		// 256
	            {true,false},		// 320
	            {true,false}		// 384
            };
        /// <summary>
        /// Samples per Frame: 1. index = LSF, 2. index = Layer
        /// </summary>
        static int[,] SamplesPerFrames = new int[,]
        {
            {	// MPEG 1
	            384,	// Layer1
	            1152,	// Layer2	
	            1152	// Layer3
            },
            {	// MPEG 2, 2.5
	            384,	// Layer1
	            1152,	// Layer2
	            576		// Layer3
            }	
        };
        /// <summary>
        /// Samples per Frame / 8
        /// </summary>
        static int[,] Coefficients = new int[,]
            {
	            {	// MPEG 1
		            12,		// Layer1	(must be multiplied with 4, because of slot size)
		            144,	// Layer2
		            144		// Layer3
	            },
	            {	// MPEG 2, 2.5
		            12,		// Layer1	(must be multiplied with 4, because of slot size)
		            144,	// Layer2
		            72		// Layer3
	            }	
            };
        /// <summary>
        /// Slot size per layer
        /// </summary>
        static int[] SlotSizes = new int[]
            {
	            4,			// Layer1
	            1,			// Layer2
	            1			// Layer3
            };
        /// <summary>
        /// Size of side information (only for Layer III)
        /// 1. index = LSF, 2. index = mono
        /// </summary>
        static int[,] SideInfoSizes = new int[,]
        {
	        // MPEG 1
	        {32,17},
	        // MPEG 2/2.5
	        {17,9}
        };

        /// <summary>
        /// Frame sync data
        /// </summary>
        private byte[] _data;
        /// <summary>
        /// Audio version id
        /// </summary>
        private byte _audioVersionId;
        /// <summary>
        /// Layer description
        /// </summary>
        private byte _layerDescription;
        /// <summary>
        /// Protection bit
        /// </summary>
        private bool _protectionBit;
        /// <summary>
        /// Bitrate used (index in array of bitrates)
        /// </summary>
        private byte _bitRateIndex;
        /// <summary>
        /// In bit per second (1 kb = 1000 bit, not 1024)
        /// </summary>
        private int _bitrate;
        /// <summary>
        /// Sampling rate used (index in array of sample rates)
        /// </summary>
        private byte _samplingRateIndex;
        private int _samplesPerSec;
        /// <summary>
        /// Padding bit
        /// </summary>
        private byte _paddingSize;
        /// <summary>
        /// Channel mode
        /// </summary>
        private byte _channelMode;
        /// <summary>
        /// 1 means lower sampling frequencies (=MPEG2/MPEG2.5)
        /// </summary>
        byte _lsf;

        public Mp3Header(byte[] data)
        {
            if ((data[0] == 0xFF) && ((data[1] & 0xE0) == 0xE0) && ((data[2] & 0xF0) != 0xF0)) // first 11 bits should be 1
            {
                _data = data;
                // Mask only the rightmost 2 bits
                _audioVersionId = (byte)((_data[1] >> 3) & 0x03);
	            if (_audioVersionId == 1)//MPEGReserved
		            throw new Exception("Corrupt mp3 header");
                _lsf = _audioVersionId == 3 ? (byte)0 : (byte)1;//MPEG1
                // Get layer (0 = layer1, 2 = layer2, ...)  [bit 13,14]
                _layerDescription = (byte)(3 - ((_data[1] >> 1) & 0x03));
                if (_layerDescription == 3)//LayerReserved
                    throw new Exception("Corrupt mp3 header");
                // Protection bit (inverted) [bit 15]
                _protectionBit = ((data[1]) & 0x01) != 0;
                // Bitrate [bit 16..19]
                _bitRateIndex = (byte)((_data[2] >> 4) & 0x0F);
                _bitrate = Bitrates[_lsf, _layerDescription, _bitRateIndex] * 1000; // convert from kbit to bit
                // Sampling rate [bit 20,21]
                _samplingRateIndex = (byte)((_data[2] >> 2) & 0x03);
	            if (_samplingRateIndex == 0x03)		// all bits set is reserved
		            throw new Exception("Corrupt mp3 header");
                _samplesPerSec = SamplingRates[_audioVersionId, _samplingRateIndex];
                // Padding bit [bit 22]
                _paddingSize = (byte)(1 * ((_data[2] >> 1) & 0x01));	// in Slots (always 1)
                // Channel mode [bit 24,25]
                _channelMode = (byte)((_data[3] >> 6) & 0x03);
            }
            else
                throw new Exception("Invalid frame sync word");
        }

        public byte[] Data
        {
            get { return _data; }
        }

        public bool IsStereo
        {
            get { return _channelMode != 3; }
        }

        public bool IsProtected
        {
            get { return _protectionBit; }
        }

        public int BitRate
        {
            get
            {
                return _bitrate;
            }
        }

        public int SampleRate
        {
            get
            {
                return _samplesPerSec;
            }
        }

        public int FrameSize
        {
            get
            {
                return (int)(((Coefficients[_lsf, _layerDescription] * _bitrate / _samplesPerSec) + _paddingSize)) * SlotSizes[_layerDescription];
            }
        }

        public double FrameDuration
        {
            get
            {
                switch (_layerDescription)
                {
                    case 3:
                        // Layer 1
                        return 384 / (SampleRate * 0.001);

                    case 2:
                    case 1:
                        if (_audioVersionId == 3)
                        {
                            // MPEG 1, Layer 2 and 3
                            return 1152 / (SampleRate * 0.001);
                        }
                        else
                        {
                            // MPEG 2 or 2.5, Layer 2 and 3
                            return 576 / (SampleRate * 0.001);
                        }

                    default:
                        // Unknown
                        return -1;
                }
            }
        }
    }
}

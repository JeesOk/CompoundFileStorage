﻿using System;
using System.IO;

/*
     The contents of this file are subject to the Mozilla Public License
     Version 1.1 (the "License"); you may not use this file except in
     compliance with the License. You may obtain a copy of the License at
     http://www.mozilla.org/MPL/

     Software distributed under the License is distributed on an "AS IS"
     basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
     License for the specific language governing rights and limitations
     under the License.

     The Original Code is OpenMCDF - Compound Document Format library.

     The Initial Developer of the Original Code is Federico Blaseotto.
 
     The code is modified to more now a days standards and upgraded to
     C# .NET 4.0 by Kees van Spelde
*/

namespace CompoundFileStorage
{
    #region Enum SectorType
    internal enum SectorType
    {
        Normal,
        Mini,
        FAT,
        DIFAT,
        RangeLockSector,
        Directory
    }
    #endregion

    internal class Sector : IDisposable
    {
        #region Fields
        public const int FreeSector = unchecked((int) 0xFFFFFFFF);
        public const int Endofchain = unchecked((int) 0xFFFFFFFE);
        public const int FATSector = unchecked((int) 0xFFFFFFFD);
        public const int DifSector = unchecked((int) 0xFFFFFFFC);
        public static int MinisectorSize = 64;
        private readonly object _lockObject = new Object();
        private readonly Stream _stream;
        private byte[] _data;
        private bool _disposed;
        #endregion

        #region Properties
        public bool DirtyFlag { get; set; }

        public bool IsStreamed
        {
            get { return (_stream != null && Size != MinisectorSize) && (Id*Size) + Size < _stream.Length; }
        }

        internal SectorType Type { get; set; }

        public int Id { get; set; }

        public int Size { get; private set; }
        #endregion

        #region Constructors
        public Sector(int size, Stream stream)
        {
            Id = -1;
            Size = size;
            _stream = stream;
        }

        public Sector(int size, byte[] data)
        {
            Id = -1;
            Size = size;
            _data = data;
            _stream = null;
        }

        public Sector(int size)
        {
            Id = -1;
            Size = size;
            _data = null;
            _stream = null;
        }
        #endregion

        #region GetData
        public byte[] GetData()
        {
            if (_data != null) return _data;
            _data = new byte[Size];

            if (!IsStreamed) return _data;
            _stream.Seek(Size + Id*(long) Size, SeekOrigin.Begin);
            _stream.Read(_data, 0, Size);

            return _data;
        }
        #endregion

        #region ReleaseData
        internal void ReleaseData()
        {
            _data = null;
        }
        #endregion

        #region ZeroData
        public void ZeroData()
        {
            _data = new byte[Size];
            DirtyFlag = true;
        }
        #endregion

        #region IDisposable Members
        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     When called from user code, release all resources, otherwise, in the case runtime called it,
        ///     only unmanagd resources are released.
        /// </summary>
        /// <param name="disposing">If true, method has been called from User code, if false it's been called from .net runtime</param>
        protected virtual void Dispose(bool disposing)
        {
            try
            {
                if (_disposed) return;
                lock (_lockObject)
                {
                    if (disposing)
                    {
                        // Call from user code...
                    }

                    _data = null;
                    DirtyFlag = false;
                    Id = Endofchain;
                    Size = 0;
                }
            }
            finally
            {
                _disposed = true;
            }
        }
        #endregion
    }
}
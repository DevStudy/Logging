﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Framework.Logging
{
    internal class Logger : ILogger
    {
        private LoggerFactory _loggerFactory;
        private string _name;
        private ILogger[] _loggers = new ILogger[0];

        public Logger(LoggerFactory loggerFactory, string name)
        {
            _loggerFactory = loggerFactory;
            _name = name;

            var providers = loggerFactory.GetProviders();
            _loggers = new ILogger[providers.Length];
            for (var index = 0; index != providers.Length; ++index)
            {
                _loggers[index] = providers[index].Create(name);
            }
        }

        public bool WriteCore(TraceType eventType, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
        {
            var result = false;
            var count = _loggers.Length;
            for (var index = 0; index != count; ++index)
            {
                result |= _loggers[index].WriteCore(eventType, eventId, state, exception, formatter);
            }
            return result;
        }

        public IDisposable BeginScope(object state)
        {
            var count = _loggers.Length;
            var scope = new Scope(count);
            for (var index = 0; index != count; ++index)
            {
                scope.SetDisposable(index, _loggers[index].BeginScope(state));
            }
            return scope;
        }

        internal void AddProvider(ILoggerProvider provider)
        {
            var logger = provider.Create(_name);
            _loggers = _loggers.Concat(new[] { logger }).ToArray();
        }

        class Scope : IDisposable
        {
            private bool disposedValue = false; // To detect redundant calls

            // first 
            private IDisposable _disposable0;
            private IDisposable _disposable1;
            private readonly IDisposable[] _disposable;

            public Scope(int count)
            {
                if (count > 2)
                {
                    _disposable = new IDisposable[count - 2];
                }
            }

            public void SetDisposable(int index, IDisposable disposable)
            {
                if (index == 0)
                {
                    _disposable0 = disposable;
                }
                else if (index == 1)
                {
                    _disposable1 = disposable;
                }
                else
                {
                    _disposable[index - 2] = disposable;
                }
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        if (_disposable0 != null)
                        {
                            _disposable0.Dispose();
                        }
                        if (_disposable1 != null)
                        {
                            _disposable1.Dispose();
                        }
                        if (_disposable != null)
                        {
                            var count = _disposable.Length;
                            for (var index = 0; index != count; ++index)
                            {
                                if (_disposable[index] != null)
                                {
                                    _disposable[index].Dispose();
                                }
                            }
                        }
                    }

                    // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                    // TODO: set large fields to null.

                    disposedValue = true;
                }
            }

            // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources. 
            // ~Scope() {
            //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            //   Dispose(false);
            // }

            // This code added to correctly implement the disposable pattern.
            public void Dispose()
            {
                // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
                Dispose(true);
                // TODO: tell GC not to call its finalizer when the above finalizer is overridden.
                // GC.SuppressFinalize(this);
            }

            internal void Add(IDisposable disposable)
            {
                throw new NotImplementedException();
            }
        }
    }
}
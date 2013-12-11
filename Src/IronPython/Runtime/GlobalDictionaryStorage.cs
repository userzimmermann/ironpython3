﻿/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;

using Microsoft.Scripting.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;

using IronPython.Runtime;
using IronPython.Compiler;

namespace IronPython.Runtime {
    class GlobalDictionaryStorage : CustomDictionaryStorage {
        private readonly Dictionary<string, PythonGlobal/*!*/>/*!*/ _globals;
        private readonly PythonGlobal[] _data;
        private PythonGlobal _path, _package, _builtins, _name;

        public GlobalDictionaryStorage(Dictionary<string, PythonGlobal/*!*/>/*!*/ globals) {
            Assert.NotNull(globals);

            _globals = globals;
        }

        public GlobalDictionaryStorage(Dictionary<string, PythonGlobal/*!*/>/*!*/ globals, PythonGlobal/*!*/[]/*!*/ data) {
            Assert.NotNull(globals, data);

            _globals = globals;
            _data = data;
        }

        protected override IEnumerable<KeyValuePair<string, object>> GetExtraItems() {
            foreach (KeyValuePair<string, PythonGlobal> global in _globals) {
                if (global.Value.RawValue != Uninitialized.Instance) {
                    yield return new KeyValuePair<string, object>(global.Key, global.Value.RawValue);
                }
            }
        }

        protected override bool? TryRemoveExtraValue(string key) {
            PythonGlobal global;
            if (_globals.TryGetValue(key, out global)) {
                if (global.RawValue != Uninitialized.Instance) {
                    global.RawValue = Uninitialized.Instance;
                    return true;
                } else {
                    return false;
                }
            }
            return null;
        }

        protected override bool TrySetExtraValue(string key, object value) {
            PythonGlobal global;
            if (_globals.TryGetValue(key, out global)) {
                global.CurrentValue = value;
                return true;
            }
            return false;
        }

        protected override bool TryGetExtraValue(string key, out object value) {
            PythonGlobal global;
            if (_globals.TryGetValue(key, out global)) {
                value = global.RawValue;
                return true;
            }

            value = null;
            return false;
        }

        public override bool TryGetBuiltins(out object value) {
            return TryGetCachedValue(ref _builtins, "__builtins__", out value);
        }

        public override bool TryGetPath(out object value) {
            return TryGetCachedValue(ref _path, "__path__", out value);
        }

        public override bool TryGetPackage(out object value) {
            return TryGetCachedValue(ref _package, "__package__", out value);
        }

        public override bool TryGetName(out object value) {
            return TryGetCachedValue(ref _name, "__name__", out value);
        }

        private bool TryGetCachedValue(ref PythonGlobal storage, string name, out object value) {
            if (storage == null) {
                if (!_globals.TryGetValue(name, out storage)) {
                    return TryGetValue(name, out value);
                }
            }

            value = storage.RawValue;
            return value != Uninitialized.Instance;
        }

        public PythonGlobal[] Data {
            get {
                return _data;
            }
        }
    }
}

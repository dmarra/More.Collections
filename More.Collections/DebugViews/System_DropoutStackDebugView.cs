using System;
using System.Diagnostics;

namespace More.Collections {
    internal sealed class System_DropoutStackDebugView<T> {
        private readonly DropoutStack<T> stack;
		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public T[] Items {
			get {
				return this.stack.ToArray();
			}
		}

		public System_DropoutStackDebugView(DropoutStack<T> stack) { 
			if (stack == null) {
				throw new ArgumentNullException("DropoutStack is null");
			}
			this.stack = stack;
		}
    }
}

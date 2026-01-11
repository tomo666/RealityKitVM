#region License
// Author: Keith Pickford
// 
// MIT License
// 
// Copyright (c) 2016 -2020 FunctionZero Ltd
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
#endregion
using RPN.Tokens;

namespace RPN.Operators {
	public class Operator : IOperator {
		private int _precedence;

		public bool IsOperator => true;
		public bool IsOperand => false;

		public Operator(OperatorType operatorType, int precedence, ShortCircuitMode shortCircuit, string asString) {
			Precedence = precedence;
			ShortCircuit = shortCircuit;
			AsString = asString;
			Type = operatorType;
		}


		//public int Precedence { get; }
		public int Precedence {
			set { _precedence = value; }
			get {
				//if(TokenType == TokenType.Function)		// Test - I think Functions have implied precedence. If they don't, I need to know what to set it to.
				//	throw new NotSupportedException("Precedence for a function");
				return _precedence;
			}
		}

		public ShortCircuitMode ShortCircuit { get; }
		public OperatorType Type { get; }

		public TokenType TokenType => TokenType.Operator;
		public string AsString { get; }

		public override string ToString() {
			return AsString;
		}
	}
}
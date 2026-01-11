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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using RPN.Operands;
using RPN.Operators;
using RPN.Tokens;

namespace RPN.Parser {
	public class ExpressionParser {
		enum State {
			None = 0,
			Operand,
			Operator,
			UnaryOperator,
			FunctionOperator,
			OpenParenthesis,
			CloseParenthesis,
			UnaryCastOperator
		}

		public const int FunctionPrecedence = 13;

		public ExpressionParser() {
			Operators = new Dictionary<string, IOperator>();

			// Register UnaryMinus ...
			UnaryMinus = RegisterUnaryOperator("UnaryMinus", 12);
			UnaryPlus = RegisterUnaryOperator("UnaryPlus", 12);
			RegisterUnaryOperator("!", 12);
			RegisterUnaryOperator("~", 12);
			RegisterOperator("*", 11);
			RegisterOperator("/", 11);
			RegisterOperator("%", 11);
			PlusOperator = RegisterOperator("+", 10);
			MinusOperator = RegisterOperator("-", 10);
			RegisterOperator("<", 9);
			RegisterOperator(">", 9);
			RegisterOperator(">=", 9);
			RegisterOperator("<=", 9);
			RegisterOperator("<<", 9);
			RegisterOperator(">>", 9);
			RegisterOperator("!=", 8);
			RegisterOperator("==", 8);
			RegisterOperator("&", 7);
			RegisterOperator("^", 6);
			RegisterOperator("|", 5);
			RegisterOperator("&&", 4, ShortCircuitMode.LogicalAnd);
			RegisterOperator("||", 3, ShortCircuitMode.LogicalOr);

			// Register operators ...
			RegisterSetEqualsOperator("=", 2); // Can do assignment to a variable.
			CommaOperator = RegisterOperator(",", 1); // Do nothing. Correct???
			OpenParenthesisOperator = RegisterOperator("(", 0, ShortCircuitMode.None, OperatorType.OpenParenthesis);
			CloseParenthesisOperator = RegisterOperator(")", 13, ShortCircuitMode.None, OperatorType.CloseParenthesis);


			Functions = new Dictionary<string, IOperator>();
		}

		private Dictionary<string, IOperator> Operators { get; }
		private Dictionary<string, IOperator> Functions { get; }

		private IOperator UnaryMinus { get; }
		private IOperator UnaryPlus { get; }
		private IOperator PlusOperator { get; }
		private IOperator MinusOperator { get; }
		private IOperator CommaOperator { get; }
		private IOperator OpenParenthesisOperator { get; }
		private IOperator CloseParenthesisOperator { get; }

		public IOperator GetNamedOperator(string strName) {
			return Operators[strName];
		}

		public IOperator RegisterOperator(
						string text,
						int precedence,
						ShortCircuitMode shortCircuit = ShortCircuitMode.None,
						OperatorType operatorType = OperatorType.Operator) {
			var op = new Operator(operatorType,
					precedence,
					shortCircuit, text
			);
			Operators.Add(text, op);
			return op;
		}

		public IOperator RegisterSetEqualsOperator(string text, int precedence) {
			var op = new Operator(OperatorType.Operator, precedence, ShortCircuitMode.None, text);
			Operators.Add(text, op);
			return op;
		}

		public IOperator RegisterUnaryOperator(string text, int precedence) {
			var op = new Operator(
					OperatorType.UnaryOperator,
					precedence,
					ShortCircuitMode.None,
					text
			);
			Operators.Add(text, op);
			return op;
		}


		public IOperator RegisterUnaryCastOperator(OperandType operandType, int precedence) {
			var text = operandType.ToString();

			var castToOperand = new Operand(operandType);

			var op = new Operator(
					OperatorType.UnaryCastOperator,
					precedence,
					ShortCircuitMode.None,
					text
			);
			Operators.Add(text, op);
			return op;
		}

		//public TokenList Parse(string expression)
		public TokenList Parse(string expression) {
			return Parse(new MemoryStream(Encoding.UTF8.GetBytes(expression ?? "")));
		}

		private State _state;
		private int _parenthesisDepth;


		//public TokenList Parse(Stream inputStream)
		public TokenList Parse(Stream inputStream) {
			_parenthesisDepth = 0;
			var tokenizer = new Tokenizer(inputStream, Operators, Functions);

			var operatorStack = new Stack<OperatorWrapper>();
			var tokenList = new TokenList();
			_state = State.None;

			var parserPosition = tokenizer.ParserPosition;
			IToken token;
			while((token = tokenizer.GetNextToken()) != null) {
				if(token is IOperator)
					token = new OperatorWrapper(TranslateOperator((IOperator)token), tokenizer.Anchor);

				//ValidateNextToken(token);

				_state = GetState(token);

				if(_state == State.UnaryCastOperator) {
					var thing = operatorStack.Pop();
					if(thing.Type != OperatorType.OpenParenthesis)
						throw new InvalidOperationException();
				}


				// TokenWrapper is Operand or OperatorWrapper. Nothing else.
				Debug.Assert((token is Operand) || (token is OperatorWrapper));

				OperatorWrapper operatorWrapper = token as OperatorWrapper;

				switch(token.TokenType) {
					case TokenType.Operator:
						switch(((IOperator)token).Type) {
							case OperatorType.Operator: {
									// Pop operators with precedence >= current operator.
									Debug.Assert(operatorWrapper != null);
									PopByPrecedence(operatorStack, tokenList, operatorWrapper.WrappedOperator.Precedence);
									if(operatorWrapper.WrappedOperator != CommaOperator)
										operatorStack.Push(operatorWrapper);
								}
								_state = State.Operator;

								break;
							case OperatorType.UnaryOperator:
								operatorStack.Push(operatorWrapper);
								break;
							case OperatorType.Function:
								operatorStack.Push(operatorWrapper);
								break;
							case OperatorType.OpenParenthesis:
								_parenthesisDepth++;
								operatorStack.Push(operatorWrapper);
								//if(lastTokenWrapper?.WrappedToken.TokenType == TokenType.Function)
								//	operatorWrapper.Tag = lastTokenWrapper.WrappedToken;			// Set the OpenParenthesis wrapper Tag to the function that precedes it.
								break;
							case OperatorType.CloseParenthesis:
								_parenthesisDepth--;
								if(_state == State.CloseParenthesis) {
									// Pop operators until an open-parenthesis is encountered.
									PopByPrecedence(operatorStack, tokenList, 1);
									operatorStack.Pop(); // Pop the open parenthesis.
								} else {

								}

								break;
							case OperatorType.UnaryCastOperator:
								operatorStack.Push(operatorWrapper);
								break;
						}

						break;
					case TokenType.Operand:
						switch(((IOperand)token).Type) {
							// No need for e.g. OperandType.Float because these cannot be created by the parser. (See Tokenizer.ParseNumberToken)
							case OperandType.Int:
							case OperandType.Long:
							case OperandType.Double:
							case OperandType.String:
							case OperandType.Bool:
							case OperandType.Variable:
							case OperandType.Null:
								tokenList.Add(token);
								break;
							default:
								throw new ArgumentOutOfRangeException();
						}

						break;
				}
			}

			PopByPrecedence(operatorStack, tokenList, 0);

			return tokenList;
		}

		/// <summary>
		/// Depending on the current parser state, a + or - operator might need to be translated to a unary + or -
		/// </summary>
		/// <param name="op"></param>
		/// <returns></returns>
		private IOperator TranslateOperator(IOperator op) {
			if((op == MinusOperator) &&
					((_state == State.Operator) || (_state == State.UnaryOperator) || (_state == State.None) ||
					 (_state == State.OpenParenthesis))) {
				return UnaryMinus;
			} else if((op == PlusOperator) &&
								 ((_state == State.Operator) || (_state == State.UnaryOperator) || (_state == State.None) ||
									(_state == State.OpenParenthesis))) {
				return UnaryPlus;
			} else {
				return op;
			}
		}


		private State GetState(IToken token) {
			switch(token.TokenType) {
				case TokenType.Operator:
					switch(((IOperator)token).Type) {
						//case TokenType.Undefined:
						//	break;
						case OperatorType.Operator:
							return State.Operator;
						case OperatorType.UnaryOperator:
							return State.UnaryOperator;
						case OperatorType.Function:
							return State.FunctionOperator;
						case OperatorType.OpenParenthesis:
							return State.OpenParenthesis;
						case OperatorType.CloseParenthesis:
							if(_state == State.UnaryCastOperator)
								return State.None;
							//if (_state != State.Operand)
							//    throw new InvalidOperationException();
							return State.CloseParenthesis;

						case OperatorType.UnaryCastOperator:
							return State.UnaryCastOperator;

					}

					break;
				case TokenType.Operand:
					switch(((IOperand)token).Type) {
						case OperandType.Sbyte:
						case OperandType.Byte:
						case OperandType.Short:
						case OperandType.Ushort:
						case OperandType.Int:
						case OperandType.Uint:
						case OperandType.Long:
						case OperandType.Ulong:
						case OperandType.Char:
						case OperandType.Float:
						case OperandType.Double:
						case OperandType.Bool:
						case OperandType.Decimal:
						case OperandType.NullableSbyte:
						case OperandType.NullableByte:
						case OperandType.NullableShort:
						case OperandType.NullableUshort:
						case OperandType.NullableInt:
						case OperandType.NullableUint:
						case OperandType.NullableLong:
						case OperandType.NullableUlong:
						case OperandType.NullableChar:
						case OperandType.NullableFloat:
						case OperandType.NullableDouble:
						case OperandType.NullableBool:
						case OperandType.NullableDecimal:
						case OperandType.String:
						case OperandType.Variable:
						//case OperandType.VSet:            // Why not these two?
						//case OperandType.Object:          // Why not these two?
						case OperandType.Null:
							return State.Operand;
					}

					break;
			}

			throw new ArgumentOutOfRangeException();
		}


		private void PopByPrecedence(Stack<OperatorWrapper> operatorStack, IList<IToken> tokenList,
				int currentPrecedence) {
			while(operatorStack.Count > 0 && operatorStack.Peek().WrappedOperator.Precedence >= currentPrecedence) {
				tokenList.Add(operatorStack.Pop());
			}
		}

	}
}
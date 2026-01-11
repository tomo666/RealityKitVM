//
//  ExpressionParser.swift
//  JBScript
//
//  Ported from FunctionZero.ExpressionParserZero.Parser (MIT License)
//  Original Author: Keith Pickford
//

import Foundation

public final class ExpressionParser {

    private enum State: Int {
        case None = 0
        case Operand
        case Operator
        case UnaryOperator
        case FunctionOperator
        case OpenParenthesis
        case CloseParenthesis
        case UnaryCastOperator
    }

    public static let FunctionPrecedence: Int = 13

    public init() {
        // Step 1: initialize stored properties that do not depend on self
        Operators = [:]
        Functions = [:]

        // Step 2: register operators (now safe to use self)
        UnaryMinus = RegisterUnaryOperator("UnaryMinus", 12)
        UnaryPlus  = RegisterUnaryOperator("UnaryPlus", 12)
        _ = RegisterUnaryOperator("!", 12)
        _ = RegisterUnaryOperator("~", 12)
        _ = RegisterOperator("*", 11)
        _ = RegisterOperator("/", 11)
        _ = RegisterOperator("%", 11)
        PlusOperator  = RegisterOperator("+", 10)
        MinusOperator = RegisterOperator("-", 10)
        _ = RegisterOperator("<", 9)
        _ = RegisterOperator(">", 9)
        _ = RegisterOperator(">=", 9)
        _ = RegisterOperator("<=", 9)
        _ = RegisterOperator("<<", 9)
        _ = RegisterOperator(">>", 9)
        _ = RegisterOperator("!=", 8)
        _ = RegisterOperator("==", 8)
        _ = RegisterOperator("&", 7)
        _ = RegisterOperator("^", 6)
        _ = RegisterOperator("|", 5)
        _ = RegisterOperator("&&", 4, .LogicalAnd)
        _ = RegisterOperator("||", 3, .LogicalOr)

        _ = RegisterSetEqualsOperator("=", 2)
        CommaOperator = RegisterOperator(",", 1)
        OpenParenthesisOperator  = RegisterOperator("(", 0, .None, .OpenParenthesis)
        CloseParenthesisOperator = RegisterOperator(")", 13, .None, .CloseParenthesis)
    }

    private var Operators: [String: IOperator] = [:]
    private var Functions: [String: IOperator] = [:]

    private var UnaryMinus: IOperator!
    private var UnaryPlus: IOperator!
    private var PlusOperator: IOperator!
    private var MinusOperator: IOperator!
    private var CommaOperator: IOperator!
    private var OpenParenthesisOperator: IOperator!
    private var CloseParenthesisOperator: IOperator!

    private var _state: State = .None
    private var _parenthesisDepth: Int = 0

    public func GetNamedOperator(_ strName: String) -> IOperator {
        return Operators[strName] ?? {
            print("GetNamedOperator: unknown operator \(strName)")
            return Operators[strName]! // unreachable fallback to keep signature
        }()
    }

    @discardableResult
    public func RegisterOperator(
        _ text: String,
        _ precedence: Int,
        _ shortCircuit: ShortCircuitMode = .None,
        _ operatorType: OperatorType = .Operator
    ) -> IOperator {
        let op = Operator(operatorType, precedence, shortCircuit, text)
        Operators[text] = op
        return op
    }

    @discardableResult
    public func RegisterSetEqualsOperator(_ text: String, _ precedence: Int) -> IOperator {
        let op = Operator(.Operator, precedence, .None, text)
        Operators[text] = op
        return op
    }

    @discardableResult
    public func RegisterUnaryOperator(_ text: String, _ precedence: Int) -> IOperator {
        let op = Operator(.UnaryOperator, precedence, .None, text)
        Operators[text] = op
        return op
    }

    @discardableResult
    public func RegisterUnaryCastOperator(_ operandType: OperandType, _ precedence: Int) -> IOperator {
        let text = String(describing: operandType)

        // Keep for parity with C# (even if currently unused)
        _ = Operand(operandType)

        let op = Operator(.UnaryCastOperator, precedence, .None, text)
        Operators[text] = op
        return op
    }

    // MARK: - Parse

    public func Parse(_ expression: String) -> TokenList {
        let data = Data((expression).utf8)
        let stream = InputStream(data: data)
        return Parse(stream)
    }

    public func Parse(_ inputStream: InputStream) -> TokenList {
        _parenthesisDepth = 0

        // Tokenizer expects operator/function registries. We pass the dictionaries.
        let tokenizer = Tokenizer(inputStream, Operators, Functions)

        var operatorStack: [OperatorWrapper] = []
        let tokenList = TokenList()
        _state = .None

        // C# had this local but unused; keep for parity
        _ = tokenizer.ParserPosition

        while let t = tokenizer.GetNextToken() {
            var token: Token = t

            if let op = token as? IOperator {
                token = OperatorWrapper(TranslateOperator(op), Int64(tokenizer.Anchor))
            }

            _state = GetState(token)

            if _state == .UnaryCastOperator {
                let thing = operatorStack.removeLast()
                if thing.`Type` != .OpenParenthesis {
                    // C# throws InvalidOperationException
                    print("InvalidOperationException")
                }
            }

            // TokenWrapper is Operand or OperatorWrapper. Nothing else.
            // (We can't Debug.Assert cross-platform; this is a sanity guard.)
            // assert((token is Operand) || (token is OperatorWrapper))

            let operatorWrapper = token as? OperatorWrapper

            switch token.TokenType {
            case .Operator:
                guard let opToken = token as? IOperator else { break }

                switch opToken.`Type` {
                case .Operator:
                    // Pop operators with precedence >= current operator.
                    if let ow = operatorWrapper {
                        PopByPrecedence(&operatorStack, tokenList, ow.WrappedOperator.Precedence)
                        if !IsSameOperator(ow.WrappedOperator, CommaOperator) {
                            operatorStack.append(ow)
                        }
                    }
                    _state = .Operator

                case .UnaryOperator:
                    if let ow = operatorWrapper {
                        operatorStack.append(ow)
                    }

                case .Function:
                    if let ow = operatorWrapper {
                        operatorStack.append(ow)
                    }

                case .OpenParenthesis:
                    _parenthesisDepth += 1
                    if let ow = operatorWrapper {
                        operatorStack.append(ow)
                    }

                case .CloseParenthesis:
                    _parenthesisDepth -= 1
                    if _state == .CloseParenthesis {
                        // Pop operators until an open-parenthesis is encountered.
                        PopByPrecedence(&operatorStack, tokenList, 1)
                        _ = operatorStack.popLast() // Pop the open parenthesis.
                    } else {
                        // no-op (parity)
                    }

                case .UnaryCastOperator:
                    if let ow = operatorWrapper {
                        operatorStack.append(ow)
                    }
                }

            case .Operand:
                guard let operand = token as? IOperand else { break }
                switch operand.`Type` {
                case .Int, .Long, .Double, .String, .Bool, .Variable, .Null:
                    tokenList.Add(token)
                default:
                    // C# throws ArgumentOutOfRangeException
                    print("ArgumentOutOfRangeException")
                }
            }
        }

        PopByPrecedence(&operatorStack, tokenList, 0)
        return tokenList
    }

    // MARK: - Operator translation

    /// Depending on the current parser state, a + or - operator might need to be translated to a unary + or -
    private func TranslateOperator(_ op: IOperator) -> IOperator {
        if IsSameOperator(op, MinusOperator) &&
            (_state == .Operator || _state == .UnaryOperator || _state == .None || _state == .OpenParenthesis) {
            return UnaryMinus
        } else if IsSameOperator(op, PlusOperator) &&
                    (_state == .Operator || _state == .UnaryOperator || _state == .None || _state == .OpenParenthesis) {
            return UnaryPlus
        } else {
            return op
        }
    }

    private func GetState(_ token: Token) -> State {
        switch token.TokenType {
        case .Operator:
            guard let op = token as? IOperator else { break }
            switch op.`Type` {
            case .Operator:
                return .Operator
            case .UnaryOperator:
                return .UnaryOperator
            case .Function:
                return .FunctionOperator
            case .OpenParenthesis:
                return .OpenParenthesis
            case .CloseParenthesis:
                if _state == .UnaryCastOperator {
                    return .None
                }
                return .CloseParenthesis
            case .UnaryCastOperator:
                return .UnaryCastOperator
            }

        case .Operand:
            guard let operand = token as? IOperand else { break }
            switch operand.`Type` {
            case .Sbyte, .Byte, .Short, .Ushort, .Int, .Uint, .Long, .Ulong, .Char, .Float, .Double, .Bool, .Decimal,
                 .NullableSbyte, .NullableByte, .NullableShort, .NullableUshort, .NullableInt, .NullableUint,
                 .NullableLong, .NullableUlong, .NullableChar, .NullableFloat, .NullableDouble, .NullableBool,
                 .NullableDecimal, .String, .Variable, .Null:
                return .Operand
            case .Object, .VSet:
                // Parity: C# did not include these; treat as out-of-range.
                break
            }
        }

        print("ArgumentOutOfRangeException")
        return .None
    }

    private func PopByPrecedence(_ operatorStack: inout [OperatorWrapper], _ tokenList: TokenList, _ currentPrecedence: Int) {
        while !operatorStack.isEmpty {
            let top = operatorStack[operatorStack.count - 1]
            if top.WrappedOperator.Precedence >= currentPrecedence {
                tokenList.Add(operatorStack.removeLast())
            } else {
                break
            }
        }
    }

    // MARK: - Identity helper

    private func IsSameOperator(_ a: IOperator, _ b: IOperator) -> Bool {
        ObjectIdentifier(a as AnyObject) == ObjectIdentifier(b as AnyObject)
    }
}

// MARK: - TokenList convenience (to keep C#-style call sites)

public extension TokenList {
    func Add(_ token: Token) {
        self.append(token)
    }
}

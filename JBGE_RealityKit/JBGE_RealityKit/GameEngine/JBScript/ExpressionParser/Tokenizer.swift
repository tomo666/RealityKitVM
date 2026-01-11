//
//  Tokenizer.swift
//  JBScript
//
//  Created/Ported by Tomohiro Kadono on 2025/12/25.
//  ------------------------------------------------
//  Based on FunctionZero.ExpressionParserZero.Parser (MIT License)
//  Original Author: Keith Pickford
//


import Foundation

public final class Tokenizer {

    private let _operators: [String: IOperator]
    private let _functions: [String: IOperator]
    private let _inputReader: String
    private var _index: String.Index

    public private(set) var ParserPosition: Int
    public private(set) var Anchor: Int

    // MARK: - Init

    public init(
        _ inputStream: InputStream,
        _ operators: [String: IOperator],
        _ functions: [String: IOperator]
    ) {
        self._operators = operators
        self._functions = functions

        // Read entire stream into String (closest StreamReader analogue)
        inputStream.open()
        defer { inputStream.close() }

        var data = Data()
        let bufferSize = 4096
        let buffer = UnsafeMutablePointer<UInt8>.allocate(capacity: bufferSize)
        defer { buffer.deallocate() }

        while inputStream.hasBytesAvailable {
            let read = inputStream.read(buffer, maxLength: bufferSize)
            if read > 0 {
                data.append(buffer, count: read)
            } else {
                break
            }
        }

        self._inputReader = String(data: data, encoding: .utf8) ?? ""
        self._index = self._inputReader.startIndex
        self.ParserPosition = 0
        self.Anchor = 0
    }

    // MARK: - Public API

    public func GetNextToken() -> Token? {
        _ = SkipWhiteSpace()
        Anchor = ParserPosition

        let nextChar = PeekNextChar()
        if nextChar == "\0" {
            return nil
        }

        if nextChar.isLetter || nextChar == "_" {
            let strToken = ParseSymbolToken()

            if strToken == "True" || strToken == "true" ||
               strToken == "False" || strToken == "false" {
                return Operand(Int64(Anchor), .Bool, Bool(strToken)!)
            } else if strToken == "Null" || strToken == "null" {
                return Operand(Int64(Anchor), .Null, nil)
            } else if let op = _operators[strToken] {
                return op
            } else if let functionToken = _functions[strToken] {
                return functionToken
            } else {
                return Operand(Int64(Anchor), .Variable, strToken)
            }

        } else if nextChar.isNumber || nextChar == "." {
            return ParseNumberToken(Int64(Anchor))

        } else if nextChar == "\"" || nextChar == "'" {
            return ParseStringToken(Int64(Anchor))

        } else {
            var nextToken = String(nextChar)
            var strToken = ""

            while AnyOperatorStartsWith(nextToken) {
                strToken = nextToken
                _ = GetNextChar()
                if PeekNextChar() == "\0" {
                    break
                }
                nextToken.append(PeekNextChar())
            }

            if let op = _operators[strToken] {
                return op
            }
        }

        return nil
    }

    // MARK: - Character handling

    private func GetNextChar() -> Character {
        if _index >= _inputReader.endIndex {
            return "\0"
        }
        let ch = _inputReader[_index]
        _index = _inputReader.index(after: _index)
        ParserPosition += 1
        return ch
    }

    private func PeekNextChar() -> Character {
        if _index >= _inputReader.endIndex {
            return "\0"
        }
        return _inputReader[_index]
    }

    private func SkipWhiteSpace() -> Int {
        var count = 0
        while PeekNextChar().isWhitespace {
            _ = GetNextChar()
            count += 1
        }
        return count
    }

    // MARK: - Token parsers

    private func ParseNumberToken(_ anchor: Int64) -> IOperand {
        var number = ""
        var hasDecimal = false

        var ch = PeekNextChar()
        while ch.isNumber || ch == "." {
            if ch == "." {
                if hasDecimal {
                    // ignored (same as C#)
                }
                hasDecimal = true
            }
            number.append(GetNextChar())
            ch = PeekNextChar()
        }

        if hasDecimal {
            return Operand(Int64(anchor), .Double, Double(number)!)
        } else {
            if let intValue = Int32(number) {
                return Operand(Int64(anchor), .Int, intValue)
            } else {
                return Operand(Int64(anchor), .Long, Int64(number)!)
            }
        }
    }

    private func ParseStringToken(_ anchor: Int64) -> IOperand {
        let delimiter = GetNextChar()
        var result = ""

        var ch = PeekNextChar()
        while ch != delimiter && ch != "\0" {
            result.append(GetNextChar())
            ch = PeekNextChar()
        }

        _ = GetNextChar() // closing delimiter
        return Operand(Int64(anchor), .String, result)
    }

    private func ParseSymbolToken() -> String {
        var strToken = ""
        var ch = PeekNextChar()

        while ch.isLetter || ch.isNumber || ch == "_" || ch == "." {
            strToken.append(GetNextChar())
            ch = PeekNextChar()
        }

        return strToken
    }

    private func AnyOperatorStartsWith(_ nextToken: String) -> Bool {
        return _operators.keys.contains { $0.hasPrefix(nextToken) }
    }
}

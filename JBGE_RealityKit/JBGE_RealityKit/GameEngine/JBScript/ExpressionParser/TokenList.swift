//
//  TokenList.swift
//  JBScript
//
//  Created/Ported by Tomohiro Kadono on 2025/12/25.
//  ------------------------------------------------
//  Based on FunctionZero.ExpressionParserZero.Parser (MIT License)
//  Original Author: Keith Pickford
//

import Foundation

public final class TokenList: Sequence {

    public var Tokens: [Token]

    public init() {
        self.Tokens = []
    }

    public init(_ tokens: [Token]) {
        self.Tokens = tokens
    }

    public func append(_ token: Token) {
        Tokens.append(token)
    }

    public var count: Int {
        Tokens.count
    }

    public subscript(index: Int) -> Token {
        Tokens[index]
    }

    public var description: String {
        return TokenService.TokensAsString(Tokens)
    }
}

public extension TokenList {
    // Conform to Sequence by forwarding to the underlying array
    func makeIterator() -> IndexingIterator<[Token]> {
        return Tokens.makeIterator()
    }

    // Convenience to match C#-style API used at call sites
    func ToString() -> String {
        return description
    }
}

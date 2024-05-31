//
// calculator-risk
// main.rs
// 31/5/2024
//
// ~thwmakos~
//
// evaluate basic arithmetic expressions using
// a simple grammar
//
// expression:
//   term
//   expression '+' term
//   expression '-' term
// term:
//   primary
//   term '*' primary
//   term '/' primary
// primary:
//   number
//   '(' expression ')'
// number:
//   floating-point-literal

use std::{io::{self, Write}, process::exit};
use std::vec;

#[derive(Debug, Copy, Clone, PartialEq)]
enum TokenKind
{
	Plus,
	Minus,
	Multiply,
	Divide,
	LeftParen,
	RightParen,
	Number,
	EndOfInput
}

// a token in the arithmetic expression
#[derive(Debug, Copy, Clone)]
struct Token
{
	kind   : TokenKind, // kind of token
	number : f32        // the number if the kind is number
}

pub struct Evaluator
{
	// tokens of the expression to be evaluated stored here
	tokens : Vec<Token>,
	// index in the tokens vector
	index : usize,
}

impl Evaluator 
{
	// evaluate the expression from the tokens in self.tokens vector
	pub fn evaluate(&mut self) -> f32
	{
		return self.expression();
	}

	fn expression(&mut self) -> f32
	{
		let mut left = self.term();
		let mut t = self.get_token();
		if t.kind == TokenKind::EndOfInput
		{
			return left;
		}

		loop 
		{
			match t.kind
			{
				TokenKind::Plus => 
				{
					left += self.term()
				},
				TokenKind::Minus => 
				{
					left -= self.term();
				}
				_ => 
				{
					// return the token and return since no more + or -
					self.return_token(t);
					return left;
				}
			}
			
			t = self.get_token();
			if t.kind == TokenKind::EndOfInput
			{
				break;
			}
		}

		left
	}

	fn term(&mut self) -> f32
	{
		let mut left = self.primary();
		let mut t = self.get_token();
		if t.kind == TokenKind::EndOfInput
		{
			return left;
		}

		loop 
		{
			match t.kind
			{
				TokenKind::Multiply =>
				{
					left *= self.primary();
				},
				TokenKind::Divide =>
				{
					let right = self.primary();
					if right == 0.0
					{
						panic!("division by zero");
					}
					left /= right;
				},
				_ =>
				{
					self.return_token(t);
					return left;
				}
			}

			t = self.get_token();
			if t.kind == TokenKind::EndOfInput
			{
				break;
			}
		}

		left
	}

	fn primary(&mut self) -> f32
	{
		let mut t = self.get_token();

		match t.kind 
		{
			TokenKind::LeftParen =>
			{
				let prim = self.expression();
				t = self.get_token();

				if t.kind != TokenKind::RightParen
				{
					panic!("right paren expected");
				}

				return prim;
			},
			TokenKind::Number =>
			{
				return t.number;
			},
			_ => panic!("primary term expected"),
		}
	}
	
	// get next token for evaluation
	fn get_token(&mut self) -> Token
	{
		match self.tokens.get(self.index)
		{
			Some(&tok) => 
			{
				self.index += 1;
				return tok.clone();
			},
			None => return Token { kind : TokenKind::EndOfInput, number : 0.0} ,
		}
	}
	
	// expression(), term() need to return their tokens
	// to be processed later
	// reduce the index and perform a sanity check
	fn return_token(&mut self, tok : Token)
	{
		self.index -= 1;

		if self.index < 0
		{
			panic!("negative index");
		}
		
		let expected_token = self.tokens.get(self.index).unwrap().clone();

		if expected_token.kind != tok.kind || expected_token.number != tok.number
		{
			panic!("wrong token returned");
		}
	}

	// tokenise the input string to a new Vec<Token> object
	fn tokenise_string(input : &str) -> Vec<Token>
	{
		let mut tokens : Vec<Token> = Vec::new();
		
		let mut chars = input.chars().peekable();

		loop 
		{
			match chars.next() 
			{
				Some(c) => match c
				{
					// eat whitespace
					'\n' | '\t' | ' ' => continue,
					// single char tokens
					'+' => tokens.push(Token { kind : TokenKind::Plus, number : 0.0 }),
					'-' => tokens.push(Token { kind : TokenKind::Minus, number : 0.0 }),
					'*' => tokens.push(Token { kind : TokenKind::Multiply, number : 0.0 }),
					'/' => tokens.push(Token { kind : TokenKind::Divide, number : 0.0 }),
					'(' => tokens.push(Token { kind : TokenKind::LeftParen, number : 0.0 }),
					')' => tokens.push(Token { kind : TokenKind::RightParen, number : 0.0 }),
				
					// start of floating point literal, read until it ends
					'0'..='9' | '.' => 
					{
						let mut number_string = String::from(c);
						// have we encountered a dot yet?
						let has_dot = c == '.';	

						loop
						{
							// peek next character
							match chars.peek()
							{
								// if digits or first dot in the literal
								// add it to the string and consume the character,
								// otherwise stop and convert to floating point number
								Some(next_char) => match next_char
								{
									'0'..='9' => 
									{
										number_string.push(next_char.clone());
										chars.next();
									},
									'.' =>
									{
										if has_dot
										{
											break;
										}
										else 
										{
											number_string.push(next_char.clone());
											chars.next();
										}
									},
									_ => break,
								},
								// nothing to do
								None => break
							}
						}

						let number : f32 = number_string.parse().expect("failed to parse number");

						tokens.push( Token { kind : TokenKind::Number, number });
					},

					_ => panic!("invalid character {} in input", c),
				},

				None => break,
			}
		}
	
		for tok in tokens.iter()
		{
			println!("{:?}", tok);
		}

		return tokens; 
	}

	// initialise a new object by tokenising the input string
	fn new(input : &str) -> Evaluator
	{
		Evaluator { tokens : Self::tokenise_string(input), index : 0 }	
	}

}

fn main() 
{
	let prompt_text = "> ";
	let result_text = "= ";
	let exit_text = "exit";

	let mut input_expr = String::new();

	loop 
	{
		print!("{prompt_text}");
		
		io::stdout().flush().expect("error flushing stdin");
		
		input_expr.clear();
		io::stdin()
			.read_line(&mut input_expr)
			.expect("could not read input");
		
		if input_expr.trim() == exit_text
		{
			break;
		}

		let mut evaluator  = Evaluator::new(&input_expr);
		let result = evaluator.evaluate();

		println!("{result_text}{result}");

	}
}

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
//   factor
//   term '*' factor
//   term '/' factor
// factor:
//   primary
//   primary '^' factor
// primary:
//   '+' primary
//   '-' primary
//   number
//   '(' expression ')'
// number:
//   floating-point-literal

use std::io::{self, Write};

type NumberType = f32;

#[derive(Debug, Copy, Clone, PartialEq)]
enum TokenKind
{
	Plus,
	Minus,
	Multiply,
	Divide,
	Exponentiate,
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
	number : NumberType // the number if the kind is number
}

// tokenise the input string to a new Vec<Token> object
// TODO: change from Err(String) to a error enum
fn tokenise_string(input : &str) -> Result<Vec<Token>, String>
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
				'^' => tokens.push(Token { kind : TokenKind::Exponentiate, number : 0.0 }),
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

				_ => return Err(format!("invalid character {} in input", c)),
			},

			None => break,
		}
	}

	if cfg!(debug_assertions)
	{
		for tok in tokens.iter()
		{
			println!("{:?}", tok);
		}
	}

	Ok(tokens)
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
	// TODO: change from Err(String) to a error enum in the functions here
	pub fn evaluate(&mut self) -> Result<NumberType, String>
	{
		self.expression()
	}

	fn expression(&mut self) -> Result<NumberType, String>
	{
		let mut left = self.term()?;
		
		let mut t = self.get_token();
		if t.kind == TokenKind::EndOfInput
		{
			return Ok(left);
		}

		loop 
		{
			match t.kind
			{
				TokenKind::Plus => 
				{
					left += self.term()?;
				},
				TokenKind::Minus => 
				{
					left -= self.term()?;
				}
				_ => 
				{
					// return the token and return since no more + or -
					self.return_token(t);
					return Ok(left);
				}
			}
			
			t = self.get_token();
			if t.kind == TokenKind::EndOfInput
			{
				break;
			}
		}

		Ok(left)
	}

	fn term(&mut self) -> Result<NumberType, String>
	{
		let mut left = self.primary()?;
		let mut t = self.get_token();
		if t.kind == TokenKind::EndOfInput
		{
			return Ok(left);
		}

		loop 
		{
			match t.kind
			{
				TokenKind::Multiply =>
				{
					left *= self.primary()?;
				},
				TokenKind::Divide =>
				{
					let right = self.primary()?;
					if right == 0.0
					{
						return Err("division by zero".to_string());
					}
					left /= right;
				},
				_ =>
				{
					self.return_token(t);
					return Ok(left);
				}
			}

			t = self.get_token();
			if t.kind == TokenKind::EndOfInput
			{
				break;
			}
		}

		Ok(left)
	}

	fn primary(&mut self) -> Result<NumberType, String>
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
					return Err("right paren expected".to_string());
				}

				return prim;
			},
			TokenKind::Number =>
			{
				return Ok(t.number);
			},
			// unary plus or minus
			TokenKind::Plus =>
			{
				return self.primary();
			},
			TokenKind::Minus =>
			{
				return Ok(-self.primary()?);
			},
			_ =>
			{
				return Err("primary term expected".to_string());
			}
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
		
		let expected_token = self.tokens.get(self.index).unwrap().clone();

		if expected_token.kind != tok.kind || expected_token.number != tok.number
		{
			panic!("wrong token returned");
		}
	}


	// initialise a new object by tokenising the input string
	fn new(input : &str) -> Result<Evaluator, String>
	{
		match tokenise_string(input)
		{
			Ok(tokens) => Ok(Evaluator { tokens, index : 0}),
			Err(e) => Err(e)
		}
		//Evaluator { tokens : Self::tokenise_string(input), index : 0 }
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

		let mut evaluator = match Evaluator::new(&input_expr)
		{
			Ok(ev) => ev,
			Err(error_msg) =>
			{
				println!("{}", error_msg);
				continue;
			}
		};

		match evaluator.evaluate()
		{
			Ok(result) =>
			{
				println!("{result_text}{result}");
			}
			Err(e) =>
			{
				println!("error: {}", e);
			}
		}
	}
}

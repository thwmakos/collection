# calculator-rs

Small project to familiarise with Rust. Evaluates basic arithmetic expressions using a simple
grammar. Compile, run and input a simple arithmetic expression. Does not currently handle 
errors very well and just panics. 

TODO: error handling and unary plus/minus

Grammar is as follows.

expression:
  term
  expression '+' term
  expression '-' term
term:
  primary
  term '*' primary
  term '/' primary
primary:
  number
  '(' expression ')'
number:
  floating-point-literal


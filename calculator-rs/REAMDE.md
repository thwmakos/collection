# calculator-rs

Small project to familiarise with Rust. Evaluates basic arithmetic expressions using a simple
grammar. Compile, run and input a simple arithmetic expression. Does not currently handle 
errors very well and just panics. 

TODO: unary minus and exponentiation are not handled correctly, for example -2^3 evaluates to
8 instead of -8, need to fix this

Grammar is as follows:

```
expression:  
    term  
    expression '+' term  
    expression '-' term  
term:  
    factor  
    term '*' factor  
    term '/' factor  
factor:
    primary
    primary '^' factor
primary:  
    '+' primary  
    '-' primary  
    number  
    '(' expression ')'  
number:  
    floating-point-literal  
```

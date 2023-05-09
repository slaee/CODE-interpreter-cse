﻿grammar CODE;

BEGIN: 'BEGIN';
CODE: 'CODE';
END: 'END';
IF: 'IF';
ELSE: 'ELSE';
WHILE: 'WHILE';
DO: 'DO';


program: NEWLINE* BEGIN CODE NEWLINE+ declarations* (executables)* NEWLINE* END CODE NEWLINE* EOF;

executables: (statement | COMMENT) NEWLINE+;

declarations: type IDENTIFIER ('=' expression)? (',' IDENTIFIER ('=' expression)?)* NEWLINE+;

statement 
	: assignment
	| variable_assignment
	| if_else_statement 
	| while_statement
	| do_while_statement
	| builtin_display
	| builtin_scan
	| COMMENT
	;

variable_declaration: type IDENTIFIER ('=' (expression))?;
variable_assignment: type IDENTIFIER;
assignment: IDENTIFIER ('=' IDENTIFIER)* '=' expression NEWLINE*;

builtin_display: 'DISPLAY' ':' expression NEWLINE*;
builtin_scan: 'SCAN' ':' IDENTIFIER (',' IDENTIFIER)*;

if_else_statement: IF '(' expression ')' NEWLINE+ BEGIN IF NEWLINE? executables* NEWLINE? END IF NEWLINE? else_if_statement* else_statement? NEWLINE?;
else_if_statement: ELSE IF '(' expression ')' NEWLINE+ BEGIN IF NEWLINE? executables* NEWLINE? END IF NEWLINE?;
else_statement: ELSE NEWLINE+ BEGIN IF NEWLINE? executables* NEWLINE? END IF NEWLINE?;

while_statement: WHILE '(' expression ')' NEWLINE+ BEGIN WHILE NEWLINE? executables* NEWLINE? END WHILE NEWLINE?;
do_while_statement: DO NEWLINE? BEGIN DO NEWLINE+ executables* NEWLINE? END DO NEWLINE? 'WHILE' '(' expression ')' NEWLINE?;

expression
	: expression factor_operator expression				# factorExpression
	| expression term_operator expression				# termExpression
	| unary_operator expression							# unaryExpression
	| expression relational_operator expression			# relationalExpression
	| expression boolean_operator expression			# booleanExpression
	| expression concat_operator expression				# concatExpression
	| newline_operator									# newlineExpression
	| escape_operator									# escapeSequenceExpression
	| constant											# constantExpression
	| IDENTIFIER										# identifierExpression
	| '(' expression ')'								# parenExpression
	| 'NOT' expression									# notExpression
	;

type: 'INT' | 'FLOAT' | 'BOOL' | 'CHAR' | 'STRING';
constant: INT | FLOAT | BOOL | CHAR | STRING;

factor_operator: '*' | '/' | '%';
term_operator: '+' | '-';
unary_operator: '+' | '-';
relational_operator: '>' | '<' | '>=' | '<=' | '==' | '<>';
boolean_operator: 'AND' | 'OR';
concat_operator: '&';
newline_operator: '$';

IDENTIFIER: [a-zA-Z_][a-zA-Z0-9_]*;
INT: [0-9]+;
FLOAT: [0-9]+('.' [0-9]+)?;
CHAR: '\'' ~('\''|'\\') '\'';
BOOL: '"TRUE"' | '"FALSE"' ;
STRING: '"' ~('"')* '"';
escape_operator: '[' . ']';

COMMENT: '#' ~[\n]* -> skip;
WHITESPACE: [\t\r]+ -> skip;
NEWLINE: '\r'? '\n'| '\r';
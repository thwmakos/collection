% GridL2Norm.m
% 1/4/2018
% ~thwmakos~

% Compute the grid norm of the 2-d matrix A
% dx, dy are the spacing in the x- and y-dimensions
function norm = GridL2Norm(A, dx, dy)
	
	% create local copy to modify
	% FIXME: this is not necessary
	X = dx * dy * (A .^ 2);
	
	% substact 1 to be constistent with the 
	% M and P in the other code.
	M = size(X, 1) - 1; % no. rows - 1
	P = size(X, 2) - 1; % no. cols - 1
	
	% the four vertices count for 1/4
	X(1,  1)        = 0.25 * X(1, 1);
	X(1,  P + 1)    = 0.25 * X(1, P + 1);
	X(M + 1, 1)     = 0.25 * X(M + 1, 1);
	X(M + 1, P + 1) = 0.25 * X(M + 1, P + 1);	

	% the rest of the four edges count for 1/2 
	X(1,           2:(end - 1)) = 0.5 * X(1,           2:(end - 1));
	X(end,         2:(end - 1)) = 0.5 * X(end,         2:(end - 1));
	X(2:(end - 1), 1)           = 0.5 * X(2:(end - 1), 1);
	X(2:(end - 1), end)         = 0.5 * X(2:(end - 1), end);
	
	%disp(X);

	norm = sqrt(sum(sum(X)));

	%norm = sqrt(X);
end

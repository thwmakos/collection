%
% 22/10/2017
%

function W_t = BrownianMotionKL(t, num_terms)
% Return a vector with the values of a BM path sampled at the elements of
% t, using the Karhunen-Loeve expansion
%   t         - Row vector of sample points
%   num_terms - Number of terms in the Karhunen-Loeve expansion

W_t = zeros(1, length(t));
Z   = randn(1, num_terms);

% It is more convenient to start the sum from k = 1.
for k = 1:num_terms
	coeff = sqrt(2) * Z(k) / ((k - 0.5) * pi);
	term  = coeff   * sin((k - 0.5) * pi * t);
    W_t   = W_t + term;
end

end

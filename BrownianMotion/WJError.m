function [e_t] = WJError(t, M, J)
%   
% 
%

e_t = zeros(1, length(t));

wjs = zeros(M, length(t));

for k = 1:M
    wjs(k, :) = BrownianMotionKL(t, J);
end

wjs         = wjs .^ 2;
column_sums = sum(wjs); 

e_t = abs( (1 / M) * column_sums - t);

end


%
% SimpleTest.m
% 18/1/2019
% ~thwmakos~
%

T = 0.005; % solve until this time 
M = 50; % grid point in x-axis
N = 8 * T * M^2; % ensure stabitily of the euler step forward in time


initial_conditon = @(x, y) (sin(2 * pi * x) .* sin(5 * pi * y));
% zerp dirichlet bc is hardcoded in Heat2D.m

% actually solve PDE (use 20 x 20 spatial grid)
[Sol, Times, X, Y] = Heat2D(M, M, N, T);

figure;
surf(X, Y, Sol(:, :, 1));
title(["T = ", num2str(Times(1))]);

figure;
surf(X, Y, Sol(:, :, end));
axis([0 1 0 1 -1 1]);
title(["T = ", num2str(Times(end))]);
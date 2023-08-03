% TestConvergence.m
% 1/4/2018
% ~thwmakos~

T  = 0.01;
%dx = [0.1, 0.1 / 2, 0.1 / 4, 0.1 / 8, 0.1 / 16, 0.1 / 32];
%dx = [0.01:0.01:0.1];
M = [10, 20, 40, 80, 160, 240];
dx = 1 ./ M;
N = 8 * T * (M .^ 2);

initial_conditon = @(x, y) (sin(2 * pi * x) .* sin(5 * pi * y)); 
exact_solution   = @(x, y, t) (exp(-29 * (pi .^ 2) * t) * initial_conditon(x, y));

Errors = zeros(1, length(dx));

for i = 1:length(dx)
	[Sol, Times, X, Y] = Heat2D(M(i), M(i), N(i), T);	
	
	% error at each timestep
	e_n = zeros(1, size(Sol, 3));

	for n = 1:size(Sol, 3)
		exact = exact_solution(X, Y, Times(n));
		e_n = GridL2Norm(Sol(:, :, n) - exact, dx(i), dx(i));
	end
	
	Errors(i) = max(e_n);

end

figure;
loglog(dx, Errors, '--o');
title('Euler Discritization in the $t$-dimension', 'interpreter', 'latex');
xlabel('$\Delta x$', 'interpreter', 'latex');
ylabel('Error', 'interpreter', 'latex');
set(gca, 'fontsize', 20);


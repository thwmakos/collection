%
% ~thwmakos~
% 22/7/2020
% 
eps = 0.1;
N = [1.2, 1.8, 2.0, 3.0];
z0 = [1 1 14];
tmax = 20;


figure;

for i = 1:length(N)
	subplot(2, 2, i);
	
	[t, z] = GetOrbit(eps, N(i), z0, tmax);
	zasy = GetAsyOrbit(eps, N(i), z0, t);

	plot(t, z(:, 1), 'Color', 'red', 'LineWidth', 2);
	hold on;
	plot(t, zasy(:, 1), 'Color', 'magenta', 'LineWidth', 2);
	hold off;
	xlabel('$t$', 'Interpreter', 'latex', 'FontSize', 16);
	xlabel('$x$', 'Interpreter', 'latex', 'FontSize', 16);
	title(sprintf('$\\varepsilon = %g, n = %g$', eps, N(i)), 'Interpreter', 'latex', 'FontSize', 18);
	legend({'Numerics', 'Asymptotics'}, 'Interpreter', 'latex', 'FontSize', 16, 'Location', 'northwest');
end

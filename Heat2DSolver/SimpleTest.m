%
% SimpleTest.m
% 18/1/2019
% ~thwmakos~
%

%T = 0.056
T = 0.063; % solve until this time 
M = 20; % grid point in x-axis
N = 8 * T * M^2; % ensure stabitily of the euler step forward in time


initial_condition = @(x, y) 2 * (sin(2 * pi * x) .* sin(5 * pi * y));

% zero dirichlet bc is hardcoded in Heat2D.m

% non-linear source
f = @(x) 20 * exp(x);

% actually solve PDE (use M x M spatial grid)
[Sol, Times, X, Y] = Heat2D_nonlinear(M, M, N, T, f, initial_condition, 0);
%
%[Sol, Times, X, Y] = Heat2D_CN_nonlinear(M, 50, 0.13, f, initial_condition);

%figure;
%surf(X, Y, Sol(:, :, 1));
%title(["T = ", num2str(Times(1))]);

% record every frame_step'th frame only
frame_step = 2;
figure('windowstate', 'maximized');
Frames = moviein(ceil(length(Times) / frame_step), gcf);
frame_counter = 1;

for n = 1:frame_step:length(Times)
	surf(X, Y, Sol(:, :, n));
	zlim([-3 8]);
	title(["T = ", num2str(Times(n), "%.4f")]);
	
	Frames(frame_counter) = getframe(gcf);
	frame_counter = frame_counter + 1;
end

movie(Frames, 2, ceil(length(Frames) / 2));

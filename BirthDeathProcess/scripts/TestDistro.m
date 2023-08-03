% TestDistro.m
% 29/3/2018
% ~thwmakos~

num_samples = 10000;
%observe_time = pi / 4;
observe_time = 0.8;
alpha = 1.2; % birth rate
beta  = 0.3; % death rate

values = zeros(1, num_samples); % value of the process at time observe_time

for i = 1:num_samples
	[T, X] = BirthDeathProcess(alpha, beta, 1, observe_time);
	values(i) = X(end);
end

XXX = values;%(values ~= 0);

figure;
hold on;
histogram(XXX, 'normalization', 'probability');
plot(0:30, Pjt(0:30, observe_time, alpha, beta), '-o');
hold off;



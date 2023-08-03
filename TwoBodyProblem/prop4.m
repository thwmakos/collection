%
% 24/5/2020
% prop5.m
% ~thwmakos~
%


eps_vals = [ 0.01, 0.01 / 2.0, 0.01 / 4.0, 0.01 / 8.0, 0.01 / 16.0, 0.01 / 32.0];
errors = zeros(1, length(eps_vals));

n = 1.9;
z0 = [1.5, 2.0, 1.5];
tmax = 50;

eps = 0.01;
[t, z] = GetOrbit(eps, n, z0, tmax);
%
asy_z = asymptotic(t, eps, z0, n);
%
err = max(abs(asy_z - z), [], 2);
plot(t, err);
ylim([0, 0.02]);

% get a numerical solution with really small epsilon to
% compare against
%[t, exact] = GetOrbit(0.00001, n, z0, tmax);

%for j = 1:length(eps_vals)
%	[t, exact] = GetOrbit(eps_vals(j), n, z0, tmax);
%	asy_z = asymptotic(t(end), eps_vals(j), z0, n);
%
%	errors(j) = abs(max(exact(end, :) - asy_z));
%end

%hold on;
%loglog(eps_vals, errors, '--o');
%loglog([eps_vals(1), eps_vals(end)], [eps_vals(1), eps_vals(end)] .^ (1.0 / (n - 1.0)));
%hold off;
%ylabel(['error at t = ', num2str(tmax)]);
%xlabel('ε')
%%plot([10^(-4), 2 * 10^(-4)], [10^(-4), 2 * 10^(-4)])

%
% t   - evaluate the asymptotic expansion at this time, can be vector
% eps - the value of ε
% z0  - intitial condition
% n   - value of n
%
function value = asymptotic(t, eps, z0, n)
	
	if n > 1 && n < 2
		value = asymptoticI(t, eps, z0, n);
	end

	if n == 2
		value = asymptoticII(t, eps, z0, n);
	end

	if n > 2
		value = asymptoticIII(t, eps, z0, n);
	end

	if n <= 1
		error('Only 1 n > 1 is supported')
	end

end

% case 1 < n < 2
function value = asymptoticI(t, eps, z0, n)
	value = zeros(length(t), 3);

	x0 = z0(1);
	y0 = z0(2);
	u0 = z0(3);

	% x 
	value(:, 1) = x0 * cos(t) + (y0 - eps * (u0 ^ (2 - n)) / (n - 2)) * sin(t);
	% y
	value(:, 2) = (y0 - eps * (u0 ^ (2 - n)) / (n - 2)) * cos(t) - x0 * sin(t);
	% u
	exponent = 1.0 / (n - 1);
	value(:, 3) = (eps ^ exponent) * ((n - 1) * t) .^ exponent;
end

% case n = 2
function value = asymptoticII(t, eps, z0, n)
	value = 0;
end

% case n > 2
function value = asymptoticIII(t, eps, z0, n)
	value = 0
end


% simple wrapper around matlab's ode solver
function [t, z] = GetOrbit(eps, n, z0, tmax)

	tspan = [0 tmax];
	solver_options = odeset('RelTol', 1e-6, 'AbsTol', 1e-6);
	[t, z] = ode45(@(t,z) odestep(t, z, eps, n), tspan, z0, solver_options);

end

% this function evaluates the vector field of the problem
% at time z, point z, for given eps, n and f
function next = odestep(t, z, eps, n)

	next = zeros(3, 1);
	next(1) = z(2); % x' = y
	next(2) = z(3) - z(1); % y' = u - x
	next(3) = -(1.0 / eps) * (z(3)^n); % eps u' = - f(t,x,y,eps) u^n  

end

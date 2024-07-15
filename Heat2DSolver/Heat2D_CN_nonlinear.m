% Heat2D_CN.m
% 11/4/2018
% ~thwmakos~

% Heat2D_CN_nonlinear.m
% 19/1/2019
% include non-linear sources 
% ~thwmakos~

% same as Heat2D.m (see there) but uses the Crank-Nicolson method, 
% that is trapezoidal discretization in the time dimension
% and the five-point laplacian in the spatial dimensions
% in this function M == P __always__
function [Sol, Times, X, Y] = Heat2D_CN_nonlinear(M, N, T, f, initial_condition)
	
	if nargin <= 4
		error('bad syntax, provide four arguments');
	end

	if nargin == 4
    	initial_condition = @(x, y) (sin(2 * pi * x) .* sin(5 * pi * y));
	end

    dirichlet_boundary_condition = 0; % DONT TOUCH THIS
    
	dx = 1.0 / M;
    dt =   T / N;
    
    Times  = 0:dt:T; 
    x = 0:dx:1;
    y = 0:dx:1;
    
    [X, Y] = meshgrid(x, y);
    
    % Sol(x, y, t)
    Sol = zeros(length(x), length(y), length(Times));
    
    
    % build the 2D five-point laplacian matrix
    % which is (M-1)^2 x (M-1)^2 dimensional
    % M - 1 is used due to the homogenous dirichelt conditions
    e = ones(M - 1 , 1);
    block = spdiags([e -4*e e], -1:1, M - 1, M - 1);
    % used below
    id = speye((M - 1)^2);
    
    % the 5-point laplacian operator is a block tri-diagonal matrix (the unknowns are arranged row by row)
	% https://www.mathworks.com/matlabcentral/fileexchange/10603--block--tri-diagonal-matrices
    Laplacian = blktridiag(block, speye(M - 1), speye(M - 1), M - 1);
    Laplacian = (1 / dx^2) * Laplacian;

	% Sparsity_pattern of Laplacian to be used with fsolve
	Sparsity_pattern = blktridiag(spdiags([e e e], -1:1, M - 1, M - 1), speye(M - 1), speye(M - 1), M - 1);
    
    % the operator is defined, we're in business
   	% options for fsolve
	
	options = optimoptions('fsolve', 'Algorithm', 'trust-region', 'Display', 'none', ...
			'JacobPattern', Sparsity_pattern, 'MaxIterations', 20);

    % apply initial condition
    Sol(:, :, 1) = initial_condition(X, Y);
    
    % initial condition may not agree with
    % the boundary ones at the corners of the square
    % in this case, the boundary conditions 
    % have priority
    Sol(1,   :,   1) = dirichlet_boundary_condition;
    Sol(end, :,   1) = dirichlet_boundary_condition;
    Sol(:,   1,   1) = dirichlet_boundary_condition;
    Sol(:,   end, 1) = dirichlet_boundary_condition;
    
    for n = 1:(length(Times) - 1)
	%for n = 1:3
        
        % boundary condition
        Sol(1,   :,   n + 1) = dirichlet_boundary_condition;
        Sol(end, :,   n + 1) = dirichlet_boundary_condition;
        Sol(:,   1,   n + 1) = dirichlet_boundary_condition;
        Sol(:,   end, n + 1) = dirichlet_boundary_condition;
        
        % try to do a trapezoidal step in time
        % need to solve a system for that
        % for details and references check out the report
        
        % collapse prevevious solution into a vector
        prev = reshape(Sol(2:end-1, 2:end-1, n), (M-1)^2, 1);
	
		% solution of F(x) = 0 is the numerical solution at the next
		% timestep
		F = @(x) (id - (dt / 2) * Laplacian) * x - (dt / 2) * f(x) - ...
			(id + (dt / 2) * Laplacian) * prev - (dt / 2) * f(prev);

        % solve the system
        sol_collapsed = fsolve(F, prev, options);	

        Sol(2:end-1, 2:end-1, n + 1) = reshape(sol_collapsed, M - 1, M -1);
    end 
end

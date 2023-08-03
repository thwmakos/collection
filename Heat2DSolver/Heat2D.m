% Heat2D.m
% 1/4/2018
% ~thwmakos~

% 2D heat equation solver, can be adapted to other parabolic 
% problems easily

% usage:
% [Sol, Times, X, Y] = Heat2D(M, P, N, T);
% contourf(X, Y, S(:, :, end))

% solve u_t = u_xx + u_yy in [0, 1]^2 for 0<= t <= T
%
% input : M - number of x-axis grid points
%         P - number of y-axis grid points
%         N - number of time steps
%         T - solve for t=0 up to T
%         initial_condition - function handle, two matrix args (x, y) 
%                              u(x, y, t=0) = initial_condition(x, y)
%                             
%         dirichlet_boundary_condition - constant for now
%                                        
%
% output : Sol - (M + 1, P + 1, N + 1)-dimensional matrix, Sol(:, :, i)
%                is the approx. solution on the grid at i-th time step
%          Times - Contains the time at each timestep
%          X, Y  - Solution meshgrid
% --------------------------------------------------------------------
function [Sol, Times, X, Y] = Heat2D(M, P, N, T, initial_condition, ...
                                    dirichlet_boundary_condition)
    
    
    if nargin < 4
        error('Provide at least the first four args!');
    end
    
    if nargin == 4
        initial_condition = @(x, y) (sin(2 * pi * x) .* sin(5 * pi * y));
        %initial_condition = @(x, y) exp(3 * x + 3 * y);
        dirichlet_boundary_condition = 0.0; %@(x, y) (0.0);
    end
    
    if nargin == 5
        dirichlet_boundary_condition = 0.0; %@(x, y) (0.0);
    end
                                
    dx = 1.0 / M;
    dy = 1.0 / P;
    dt =   T / N;
    
    Times  = 0:dt:T; 
    x = 0:dx:1;
    y = 0:dy:1;
    
    [X, Y] = meshgrid(x, y);
    
    % Sol(x, y, t)
    Sol = zeros(length(x), length(y), length(Times));
    
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
        
        % holds the new 5-point laplacian
        laplacian = zeros(length(x), length(y));
        % solution at the current time step
        S = Sol(:, :, n);
        
        for m = 1:M + 1
            for p = 1:P + 1
                % edges
                if m == 1 || p == 1 || m == M + 1 || p == P + 1
                    laplacian(m, p) = dirichlet_boundary_condition;%((m - 1) * dx, (p - 1) * dy);
                
                % interior
                else
                    laplacian(m, p) = (1 / dx^2) * (S(m - 1, p) - 2 * S(m, p) + S(m + 1, p)) +  ...
                        (1 / dy^2) * (S(m, p - 1) - 2 * S(m, p) + S(m, p + 1));
                end
            end
        end
        
        % Euler step forward in time
        Sol(:, :, n + 1) = S + dt * laplacian;
        
        % enforce  boundary condition
        % this is not necessary if the dirichlet boundary conditions
        % are homogenous
        Sol(1,   :,   n + 1) = dirichlet_boundary_condition;
        Sol(:,   1,   n + 1) = dirichlet_boundary_condition;
        Sol(end, :,   n + 1) = dirichlet_boundary_condition;
        Sol(:,   end, n + 1) = dirichlet_boundary_condition;
    end  
end

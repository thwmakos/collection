function fold

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
% Matlab Simulation of the fold PDE normal form
%
% u_t = u_{xx} - v + u^2  
% v_t = epsilon * ( v_{xx} - 1 ) 
%
% on the domain [0,L] with Neumann boundary conditions. We use spatial
% semi-discretization via finite differences and MATLAB's standard very
% robust ODE solvers to make adaption to other space discretizations easy
%
% Note: higher-order terms, changing initial data, similar equations, 
% spectral Galerkin, etc can all be implemented based upon this code.
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%  

% Basics
epsilon = 0.01;     % parameter epsilon
tspan   = [0;2.1];  % integration time interval 
L = 5;              % interval length                       
N = 100;            % number of mesh points 
h = L/(N-1);        % mesh step
x = 0:h:L;          % mesh points

% initial data   
u0 = -1 * ones(1,N) + 0.25 * rand(1,N); 
v0 = -1 * ones(1,N) - 0.25 * rand(1,N); 
w0 = [u0 v0];

% construct the diffusion/Laplacian matrix
evec = ones(N,1);
A1   = spdiags([evec, -2 * evec, evec], -1:1, N, N);
A2   = spdiags(epsilon * [evec, -2 * evec, evec], -1:1, N, N);

A1(1, 1) = -1;
A1(N, N) = -1; 
A2(1, 1) = -epsilon; 
A2(N, N) = -epsilon; %Neumann bc

A = (1 / h^2) * [A1 zeros(size(A1)); zeros(size(A2)) A2];

% calculate numerical solution
[t, res] = ode15s(@vf, tspan, w0);

figure(1); clf;

% plot numerical solution
for k=1:length(t)
        plot(x, res(k,1:N),'r'), hold on
        plot(x, res(k,N+1:2*N),'b'), hold off
        axis([x(1), x(N), -2, 50])
		title(['t = ' num2str(t(k))])
        pause(0.05) 
end

        % Define the ODEs function as nested function, using the parameters
        function wdot = vf(t,w)
            u = w(1:N); v = w(N+1:2*N);
            w(length(w));  
            wdot = A * w + [-v + u.* u; -epsilon * ones(size(v))]; 
        end
end

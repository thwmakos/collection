function shishkova

epsilon = 0.05;     % parameter epsilon
initial_time = -0.9; %
end_time = 1;
tspan   = [0; (abs(initial_time) + end_time) / epsilon];  % integration time interval 
L = 5;              % interval length                       
N = 100;            % number of mesh points 
h = L/(N-1);        % mesh step
x = 0:h:L;          % mesh points

animation_duration = 3; % in seconds

% initial data   
u0 =  0.5 * ones(1, N) + 0.45 * rand(1, N); 
v0 =  0.5 * ones(1, N) - 0.45 * rand(1, N); 
s0 = initial_time * ones(1, N) + 0.1 * rand(1, N);
w0 = [u0 v0 s0];

% construct the diffusion/Laplacian matrix
evec = ones(N,1);
A1   = spdiags([evec, -2 * evec, evec], -1:1, N, N);
A2   = spdiags(epsilon * [evec, -2 * evec, evec], -1:1, N, N);

A1(1, 1) = -1;
A1(N, N) = -1; 
A2(1, 1) = -epsilon; 
A2(N, N) = -epsilon; %Neumann bc

% calculate numerical solution
[t, res] = ode15s(@vf, tspan, w0);

figure(1); clf;

% plot numerical solution
for k=1:length(t)
        plot(x, res(k,1:N),'r'), hold on
        plot(x, res(k,N+1:2*N),'b')
		plot(x, res(k, 2*N+1:3*N), 'magenta'), hold off
        axis([x(1), x(N), -2, 10])
		title(['t = ' num2str(t(k))])

		if(k < length(t)) % avoid out of bounds index
			pause((t(k + 1) - t(k)) / tspan(end) * animation_duration) 
		end
		%max(res(k, 1:N))
end

        % Define the ODEs function as nested function, using the parameters
        function wdot = vf(t,w)
            u = w(1:N); 
			v = w(N + 1 : 2 * N);
			s = w(2*N + 1 : 3 * N);

            %w(length(w));

			u_out = A1 * u + s .* u - v;
		   	v_out = A1 * v + s .* v + u;
			s_out = A2 * s + epsilon * ones(size(v));	
			wdot  = [u_out; v_out; s_out];
            %wdot = A * w + [-v + u.* u; -epsilon * ones(size(v))]; 
        end
end

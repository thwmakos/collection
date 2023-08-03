% BirthDeathProcess.m
% 29/3/2018
% ~thwmakos~

function [T, X] = BirthDeathProcess(alpha, beta, X0, max_time)
%BIRTHDEATHPROCESS Simulate birth death process until max_time or zero.

	X = [X0];
	T = [0];

	while T(end) <= max_time
		current_pop  = X(end);
		current_time = T(end);

		if current_pop == 0
			break;
		end

		rate = current_pop * (alpha + beta); 
		
		% reverse rate because exprnd uses the other
		% parametrization of the exponential distro
		wait_time = exprnd(1.0 / rate);

		T = [T, current_time + wait_time];

		change = 0;

		if rand() <= alpha / (alpha + beta)
			change =  1;
		else
			change = -1;
		end

		X = [X, current_pop + change];

	end
end


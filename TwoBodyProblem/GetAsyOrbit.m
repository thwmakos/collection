%
% ~thwmakos~
% 22/7/2020
% 

function z = GetAsyOrbit(eps, n, z0, t)
	x0 = z0(1);
	y0 = z0(2);
	u0 = z0(3);
	
	z = zeros(length(t), 3);
	
	if n < 2.0 
		z(:, 1) = x0 * cos(t) + (y0 - eps * (u0 ^ (2 - n)) / (n - 2)) * sin(t);
		z(:, 2) = (y0 - eps * (u0 ^ (2 - n)) / (n - 2)) * cos(t) - x0 * sin(t);
		z(:, 3) = eps .^ (1 / (n - 1)) * ((n - 1) * t) .^ (1 / (1 - n));
	elseif n == 2.0 
		z(:, 1) = x0 * cos(t) + (y0 - eps * reallog(eps)) * sin(t);
		z(:, 2) = (y0 - eps * reallog(eps)) * cos(t) - x0 * sin(t);
		z(:, 3) = eps ./ t;
	elseif n > 2.0
		coeff1 = x0 - ((eps ^ (1 / (n - 1))) * (n - 1) / (2 * n - 3)) * (t .^ 2) .* (((n - 1) * t) .^ (1 / (n - 1))) .* ...
			hypergeom((2 * n - 3) / (2 * n - 2), [3 / 2; (4 * n - 5) / (2 * n - 2)], - t .^ 2 / 4);
		coeff2 = y0 + ((eps ^ (1 / (n - 1))) * (n - 1) / (n - 2)) * t .* (((n - 1) * t) .^ (1 / (n - 1))) .* ...
			hypergeom((n - 2) / (2 * n - 2), [1 / 2; (3 * n - 4) / (2 * n - 2)], - t .^ 2 / 4);
		
		z(:, 1) = coeff1 .* cos(t) + coeff2 .* sin(t);
		z(:, 2) = coeff2 .* cos(t) - coeff1 .* sin(t);
		z(:, 3) = eps .^ (1 / (n - 1)) * ((n - 1) * t) .^ (1 / (1 - n));
	else
		error('invalid n');
	end
end

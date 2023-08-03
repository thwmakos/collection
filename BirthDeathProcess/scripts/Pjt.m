% Pjt.m
% 29/3/2018
% ~thwmakos~

function ret = Pjt(j, t, alpha, beta)
	
	ret = zeros(1, length(j));

	a = alpha;
	b = beta;
	
	l = a - b;
	temp = exp(l * t);

	for i = 1:length(j)
		if j(i) == 0
			ret(i) = (b - b * exp(-l * t)) / (a - b * exp(-l * t));
		else
			ret(i) = (l ^ 2 * temp) / ((a * temp - b) .^ 2) * ((a * temp - a) / (a * temp - b))^(j(i)-1);
		end
	end	

end

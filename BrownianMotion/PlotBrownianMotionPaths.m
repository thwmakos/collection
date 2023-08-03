%
% 22/10/2017
%

figure; 
hold on;
title('Brownian motion paths in [0, 1], J = 5000');
xlabel('t')
ylabel('W(t)')

t = linspace(0, 1, 2000);
J = floor(linspace(100, 5000, 10));
for k = 1:10
    y = BrownianMotionKL(t, J(k));
    plot(t, y .^ 2);
    pause(2);
end

plot([0 1], [0 0], 'color', 'black');

%ylim([-2.5 2.5]);

hold off;
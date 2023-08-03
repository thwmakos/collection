J = floor(logspace(0, 3, 15)) + 1;
t = linspace(0, 1, 100);

figure;
hold on;

for ii = 1:length(J)
    y = WJError(t, 4000, J(ii));
    plot(t, y);
end

hold off;
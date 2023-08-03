%
% ~thwmakos~
% 18/7/2020
%


enable_draw_expected = true;

create_plots(enable_draw_expected);
%create_X_plots(enable_draw_expected);
%create_Y_plots(enable_draw_expected);

function [] = create_plots(enable_draw_expected)

	xdata = jsondecode(fileread('~/Temp/x-data.json'));
	ydata = jsondecode(fileread('~/Temp/y-data.json'));
	
	figure('DefaultAxesFontSize', 12);
	for i = 1:4
		subplot(2, 2, i);

		loglog(xdata(i).data.epsVals, xdata(i).data.errVals, '--o', 'LineWidth', 2, 'Color', [1 0.76 1]);
		hold on;
		loglog(ydata(i).data.epsVals, ydata(i).data.errVals, '--o', 'LineWidth', 2, 'Color', 'magenta');
			
		legend_text_cell = {'$\Delta x (t_\ast, \varepsilon)$', '$\Delta y (t_\ast, \varepsilon)$'};
		if enable_draw_expected
			plot_expected(xdata(i).n, xdata(i).data.epsVals(3:4));
			%legend_text_cell{end + 1} = 'expected error';
			legend_text_cell{end + 1} = get_legeng_string(xdata(i).n);
		end

		hold off;
		
		title(['$n = ', num2str(xdata(i).n), '$'], 'Interpreter', 'latex', 'FontSize', 18);
		legend(legend_text_cell, 'Interpreter', 'latex', 'Location', 'northwest', 'FontSize', 14);
		
		xlabel('$\varepsilon$', 'Interpreter', 'latex', 'FontSize', 18);
		%ylabel('$| y(t_1, \varepsilon) - y_{asymptotic}(t_1, \varepsilon)|$', 'Interpreter', 'latex', 'FontSize', 18);
		%ylabel('Error', 'Interpreter', 'latex', 'FontSize', 18);
	end
	
	%print('-bestfit', '~/Temp/error_plots', '-dpdf');
end

function [] = plot_expected(n, x_data)
	y_data = zeros(1, length(x_data));
	
	if n >= 1 && n <= 1.5
		y_data = x_data .^ 2;
	elseif n <= 2.0
		y_data = x_data .^ (1 / (n - 1));
	else
		y_data = x_data ./ 10;
	end
	
	loglog(x_data, y_data, 'LineWidth', 1);
	
end

function legend_string = get_legeng_string(n)
	if n >= 1 && n <= 1.5
		legend_string = '$\mathcal{O}(\varepsilon^2)$';
	elseif n < 2.0
		exponent = 1.0 / (n - 1.0);
		legend_string = ['$\mathcal{O}\big(\varepsilon^{' num2str(exponent)  '})$'];
	elseif n >= 2.0 
		legend_string = '$\mathcal{O}( \varepsilon )$';
	else
		error('Invalid value for n!');
	end
end

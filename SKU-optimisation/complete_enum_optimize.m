function best_alloc = total_enum_optimize(start_alloc, Orders, Dist)
%TOTAL_ENUM_OPTIMIZE Find best allocation by comparing pair swaps 
%   start_alloc - An initial allocation vector(shelf->SKU)
%	Orders      - A matrix containg the orders
%	Dist        - The distance matrix

current_alloc    = start_alloc; % we will be impoving this allocation
current_distance = total_order_walk(Orders, start_alloc, Dist); 

disp(current_distance);

swap_happended = true;        % is there a swap that decreases total distance?

pairs     = nchoosek(1:length(current_alloc), 2); % generate all pairs of shelves
num_pairs = nchoosek(length(current_alloc), 2);   % number of pairs

diffs = zeros(num_pairs, 1);

while swap_happended == true
	swap_happended = false; % reset flag

	% for each pair of shelves
	for i = 1:num_pairs
		alloc = current_alloc; % create a working copy
		
		% find orders that will be affected by the swap
		item1 = current_alloc(pairs(i, 1));
		item2 = current_alloc(pairs(i, 2));
		
		tf = any((Orders == item1 | Orders == item2), 2);
		affected_orders = Orders(tf, :); % this now has orders containing item1 or item2
		
		% affected orders cost before swap
		d1 = total_order_walk(affected_orders, alloc, Dist);
		
		% swap the items
		alloc([pairs(i, 1) pairs(i, 2)]) = alloc([pairs(i, 2) pairs(i, 1)]);

		% affected orders cost after swap
		d2 = total_order_walk(affected_orders, alloc, Dist);

		% store the differece, d1-d2 is the decrease in total distance
		% if we actually proceed with the swap (if d1-d2 < 0 we are worse)
		diffs(i) = d1 - d2; 
	end
	
	% now, let's see which swap is the best	
	[m, index] = max(diffs);
	
	% some swap actually decreases total distance
	if m > 0
		% update allocation
		current_alloc([pairs(index, 1) pairs(index, 2)]) = current_alloc([pairs(index, 2) pairs(index, 1)]);
		
		% printf is the best debugger!	
		disp(sprintf('found swap at index %d, improvement %d\n', index, m));	

		% set the flag to true
		swap_happended = true;	
	end
end

best_alloc = current_alloc;

end


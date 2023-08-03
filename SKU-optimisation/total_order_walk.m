function total_walk = total_order_walk(Orders, Allocation, DistanceMatrix)
%TOTAL_ORDER_WALK Calculate total walking distance for a list of orders. 
%	Arguments:
%	Orders         - The matrix containing the orders.
%	ItemLocations  - The allocation vector
%	DistanceMatrix - The matrix containing the distance between selves. The last row contains the distance between the entrance and the selves.

ItemLocations = zeros(1, length(Allocation));

% generate item->shelf mapping
for i = 1:length(ItemLocations)
	ItemLocations(i) = find(Allocation == i);
end

%order_dist = zeros(size(Orders, 1), 1); 
total_walk = 0;
for i = 1:size(Orders, 1)
	% the least walking distance for a single order is calculated by order_walk
	% the arguments passed to order_walk are not modified by the function, so
	% they are not copied, therefore passed by reference
%	order_dist(i) = order_walk(Orders(i, :), ItemLocations, DistanceMatrix);
	total_walk = total_walk + order_walk(Orders(i, :), ItemLocations, DistanceMatrix);
end

%total_walk = sum(order_dist);

end


function walk_distance = order_walk(order_items, Loc, Dist)

%ORDER_WALK Calculate walk distance for a single order.
%   Arguments:
%	items - vector of items in this order
%	Loc   - a vector such that Loc(ItemIndex) = Item Self, this is different
%			than the allocation vector which maps Self->Item
%	Dist  - distance matrix 

% remove any zero items
items = order_items(order_items ~= 0);

% find the optimal picking order order by
% testing all posible cases
% since each order has at most 3 items 
% this will be fast
paths = perms(items);
dists = zeros(size(paths, 1), 1);
 
% for each possible path
for i = 1:length(dists)
	% enter the warehouse
	dists(i) = Dist(end, Loc(paths(i, 1)));

	% pick up the items
	for j = 1:length(items)-1
		dists(i) = dists(i) + Dist(Loc(paths(i, j)), Loc(paths(i, j + 1)));
	end	

	% return to the exit
	dists(i) = dists(i) + Dist(end, Loc(paths(i, end)));
end

%dists
walk_distance = min(dists);
end


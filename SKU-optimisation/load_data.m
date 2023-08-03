% Load csv data.

% load distance matrix
Dist = csvread('data/DistanceMatrix.csv', 1, 1);
% load current allocation
CurrAlloc = csvread('data/CurrentAllocation.csv', 1, 1);

% load orders
Orders = csvread('data/Orders.csv', 1, 1);

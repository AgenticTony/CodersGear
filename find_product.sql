-- Find the problematic product
SELECT ProductId, ProductName, CategoryId 
FROM Products 
WHERE ProductName LIKE '%I Turn Coffee Into Code%';

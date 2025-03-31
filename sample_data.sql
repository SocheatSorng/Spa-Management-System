-- Insert sample services
INSERT INTO tbService (ServiceName, Description, Price, ImagePath) VALUES
('Swedish Massage', 'Traditional massage using long strokes and kneading techniques', 75.00, 'Images/Services/swedish-massage.jpg'),
('Deep Tissue Massage', 'Intense massage targeting deep muscle layers', 90.00, 'Images/Services/deep-tissue.jpg'),
('Hot Stone Massage', 'Massage using heated stones for deep relaxation', 95.00, 'Images/Services/hot-stone.jpg'),
('Aromatherapy Massage', 'Massage with essential oils for relaxation', 85.00, 'Images/Services/aromatherapy.jpg'),
('Facial Treatment', 'Deep cleansing and rejuvenating facial', 65.00, 'Images/Services/facial.jpg'),
('Body Scrub', 'Full body exfoliation treatment', 70.00, 'Images/Services/body-scrub.jpg'),
('Foot Reflexology', 'Pressure point massage focusing on feet', 50.00, 'Images/Services/reflexology.jpg'),
('Thai Massage', 'Traditional Thai stretching and pressure point massage', 85.00, 'Images/Services/thai-massage.jpg'),
('Couples Massage', 'Relaxing massage for two people', 150.00, 'Images/Services/couples-massage.jpg'),
('Anti-Aging Facial', 'Premium facial targeting signs of aging', 95.00, 'Images/Services/anti-aging.jpg');

-- Insert sample foods (consumables)
INSERT INTO tbConsumable (Name, Description, Price, Category, StockQuantity, ImagePath) VALUES
('Fresh Fruit Salad', 'Assorted seasonal fruits', 8.50, 'Foods', 20, 'Images/Consumables/fruit-salad.jpg'),
('Caesar Salad', 'Classic caesar salad with grilled chicken', 12.00, 'Foods', 15, 'Images/Consumables/caesar-salad.jpg'),
('Vegetable Wrap', 'Fresh vegetables in whole wheat wrap', 10.00, 'Foods', 15, 'Images/Consumables/veggie-wrap.jpg'),
('Quinoa Bowl', 'Healthy quinoa with roasted vegetables', 13.50, 'Foods', 12, 'Images/Consumables/quinoa-bowl.jpg'),
('Greek Yogurt Parfait', 'Yogurt with granola and berries', 7.50, 'Foods', 18, 'Images/Consumables/yogurt-parfait.jpg'),
('Chicken Sandwich', 'Grilled chicken with avocado', 11.00, 'Foods', 15, 'Images/Consumables/chicken-sandwich.jpg'),
('Sushi Roll', 'Fresh vegetable sushi roll', 14.00, 'Foods', 10, 'Images/Consumables/sushi-roll.jpg'),
('Protein Bowl', 'Mixed grains with grilled tofu', 13.00, 'Foods', 12, 'Images/Consumables/protein-bowl.jpg'),
('Mediterranean Plate', 'Hummus, falafel, and pita', 12.50, 'Foods', 10, 'Images/Consumables/mediterranean-plate.jpg'),
('Energy Bar', 'Homemade nuts and dried fruit bar', 5.00, 'Foods', 25, 'Images/Consumables/energy-bar.jpg');

-- Insert sample drinks (consumables)
INSERT INTO tbConsumable (Name, Description, Price, Category, StockQuantity, ImagePath) VALUES
('Green Detox Smoothie', 'Spinach, apple, and ginger smoothie', 7.50, 'Drinks', 20, 'Images/Consumables/green-smoothie.jpg'),
('Herbal Tea', 'Organic herbal tea blend', 4.50, 'Drinks', 30, 'Images/Consumables/herbal-tea.jpg'),
('Fresh Coconut Water', 'Natural coconut water', 6.00, 'Drinks', 25, 'Images/Consumables/coconut-water.jpg'),
('Berry Blast Smoothie', 'Mixed berry protein smoothie', 8.00, 'Drinks', 20, 'Images/Consumables/berry-smoothie.jpg'),
('Cucumber Mint Water', 'Infused water with cucumber and mint', 4.00, 'Drinks', 25, 'Images/Consumables/cucumber-water.jpg'),
('Ginger Lemon Tea', 'Fresh ginger and lemon tea', 5.00, 'Drinks', 30, 'Images/Consumables/ginger-tea.jpg'),
('Mango Lassi', 'Yogurt-based mango drink', 6.50, 'Drinks', 15, 'Images/Consumables/mango-lassi.jpg'),
('Sparkling Water', 'Premium sparkling mineral water', 3.50, 'Drinks', 40, 'Images/Consumables/sparkling-water.jpg'),
('Kombucha', 'Fermented probiotic tea drink', 6.00, 'Drinks', 20, 'Images/Consumables/kombucha.jpg'),
('Fresh Orange Juice', 'Freshly squeezed orange juice', 5.50, 'Drinks', 25, 'Images/Consumables/orange-juice.jpg'); 
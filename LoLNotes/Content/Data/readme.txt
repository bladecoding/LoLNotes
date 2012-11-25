ChampData.json is from '\League of Legends\RADS\projects\lol_air_client\releases\0.0.0.223\deploy\assets\data\gameStats\gameStats_en_US.sqlite'.
Use something like sqlite manager for firefox and run the following query to generate the data for ChampData.json.

SELECT 
'{
' || group_concat('    "'|| id || '":"' || name || '",
', '') || '}' FROM champions
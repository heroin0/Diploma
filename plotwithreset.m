clear
clc
load plotwithreset.txt;
x=1:length(plotwithreset);
plot(x,plotwithreset);
title('Алгоритм с перезапусками');
xlabel('итерации')
ylabel('стоимость конфигурации')
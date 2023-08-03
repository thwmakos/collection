%% asymptics check 1<n<2
syms n epsilon
x0=1;
y0=1;
r0=1;
u0=10;
ic=r0^(1-n)*epsilon;
xin=x0+epsilon*y0/(n-1)*(r0^(1-n)-u0^(1-n));
yin=y0-epsilon*(x0/(n-1)*(r0^(1-n)-u0^(1-n))-(r0^(2-n)-u0^(2-n))/(n-2));
syms epsilon1 x(epsilon1) y(epsilon1) r(epsilon1)
eq1=diff(x(epsilon1),epsilon1)-y(epsilon1)/(n-1)==0;
eq2=diff(y(epsilon1),epsilon1)-(r(epsilon1)-x(epsilon1))/(n-1)==0;
eq3=diff(r(epsilon1),epsilon1)+r(epsilon1)/epsilon1/(n-1)==0;
An=dsolve(eq1,eq2,eq3,x(ic)==xin,y(ic)==yin,r(ic)==r0,'epsilon1');
X=An.x; 
Y=An.y;
Xout=subs(X,epsilon1,1);
Yout=subs(Y,epsilon1,1);
syms s t
Ix=int(sin(t-s)*((n-1)*s+epsilon*u0^(1-n))^(1/(1-n)),s, (1-epsilon*u0^(1-n)),t);
Xt=Xout*cos(t-1/(n-1)+epsilon*u0^(1-n)/(n-1))+Yout*sin(t-1/(n-1)+epsilon*u0^(1-n)/(n-1))+epsilon^(1/(n-1))*Ix;

%% 1<n<2
Ex1=x0*cos(1/(n-1))+y0*sin(1/(n-1))-epsilon*(y0*u0^(1-n)*cos(1/(n-1))/(n-1)-x0*u0^(1-n)*sin(1/(n-1))/(n-1)+u0^(2-n)*sin(1/(n-1))/(n-2));
Ey1=y0*cos(1/(n-1))-x0*sin(1/(n-1))+epsilon*(x0*u0^(1-n)*cos(1/(n-1))/(n-1)+y0*u0^(1-n)*sin(1/(n-1))/(n-1)-u0^(2-n)*cos(1/(n-1))/(n-2));
Ext1=x0*cos(t)+(y0-epsilon*u0^(2-n)/(n-2))*sin(t)
% 1<n<2
k1=1.4;
Nx1=subs(X,[n,epsilon1],[k1,1]);
Ny1=subs(Y,[n,epsilon1],[k1,1]);
Nex1=subs(Ex1,n,k1);
Ney1=subs(Ey1,n,k1);
Erx1=abs(Nx1-Nex1);
Ery1=abs(Ny1-Ney1);
%
figure;
fplot(Erx1,epsilon,[0.001,0.01],'g-', 'linewidth',2)
ylabel('epsilon');xlabel('abs Error of Xout')
title(['n=' num2str(k1) ',  Xout']);set(gca,'FontSize',16);  
view([90,-90])
figure;
fplot(Ery1,epsilon,[0.001,0.1],'b-', 'linewidth',2)
ylabel('epsilon');xlabel('abs Error of Yout')
title(['n=' num2str(k1) ', Yout']);set(gca,'FontSize',16);  
view([90,-90])
%%
ke1=0.001;
Nxt1=subs(Xt,[n,epsilon1,epsilon],[k1,1,ke1])
Next1=subs(Ext1,[n, epsilon],[k1,ke1])
Erxt1=abs(Nxt1-Next1);
%
figure;
fplot(Erxt1,t,[0,10],'r-.', 'linewidth',2)
ylabel('t');xlabel('abs Error of X(t, eps)')
title(['n=' num2str(k1) ',  epsilon=' num2str(ke1)]);set(gca,'FontSize',16);  
view([90,-90])

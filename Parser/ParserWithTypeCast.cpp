/* strtof example */
#include <stdio.h>      /* printf, NULL */
#include <stdlib.h>     /* strtof */

int main ()
{
  char szOrbits[] = "0.15 0.1756";
  char* pEnd;
  float f1, f2;
  f1 = strtof (szOrbits, &pEnd);
  f2 = strtof (pEnd, NULL);
  printf("%f\n",f1);
  printf("%f",f2);
  return 0;
}

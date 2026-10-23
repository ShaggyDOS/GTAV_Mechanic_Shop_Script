using System;
class Harness {
/*<source:SupplierPhysicalRange>*/
static void Main(){int n=0;foreach(float radius in new[]{90f,200f,700f}) foreach(float d in new[]{0f,79f,80f,89f,90f,100f,150f,199f,200f,201f,699f,700f,701f}) {if(SupplierPhysicalRange(d*d,radius)!=(d<=radius))throw new Exception("range gap");n++;}if(SupplierPhysicalRange(float.NaN,200))throw new Exception("invalid range");n++;Console.WriteLine($"range {n}/{n} PASS; shared physical/virtual boundary, GTA NOT RUN");}
}

cd ExscriboAPI
rm -r asset-output
dotnet publish -r linux-x64 -c Release && dotnet lambda package --output-package asset-output/function.zip      

cd ..

cd StepFunctionLambda
rm -r asset-output
dotnet publish -r linux-x64 -c Release && dotnet lambda package --output-package asset-output/function.zip      

cd ..


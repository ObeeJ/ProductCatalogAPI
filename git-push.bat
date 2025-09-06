@echo off
git init
git remote add origin https://github.com/ObeeJ/ProductCatalogAPI.git
git add .
git commit -m "Initial commit: Production-grade Web API with concurrency control"
git branch -M main
git push -u origin main
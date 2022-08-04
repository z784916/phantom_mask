# Response
## API Document (required)

  ### 1.List all pharmacies open at a specific time and on a day of the week if requested.
  [GET] /ListPharmaciesByTime
  -    day: 指星期幾，1代表星期一，2代表星期二，以此列推
  -    time: 請用HH:mm格式
  ```
  {
      "day":"1",
      "time":"07:21"
  }
  ```


  ### 2. List all masks sold by a given pharmacy, sorted by mask name or price.
  [GET] /ListMasksByPharmacy 
  -    name: 藥局名稱
  -    sortBy: 填入"name"或是"price"
  ```
  {
      "name":"DFW Wellness",
      "sortBy":"price"
  }
  ```

  ### 3.List all pharmacies with more or less than x mask products within a price range.
  [GET] /ListPharmacyByPriceRange
  -	x: x個口罩產品
  -	moreOrLess: 填入"more"或是"less""
  -	priceRange: 填入兩個數字，以"~"符號隔開
  ```
  {
      "x": "2",
      "moreOrLess": "mrore",
      "priceRange": "10~20"
  }
  ```
  

  ### 4.The top x users by total transaction amount of masks within a date range.
  [GET] /ListUserByDateRange
  - x: 前X個用戶
  - startTime: 開始時間，格式yyyy-MM-dd HH:mm:ss
  - endTime: 結束時間，格式yyyy-MM-dd HH:mm:ss
  ```
  {
      "x": "2",
      "startTime": "2021-01-17 05:41:10",
      "endTime": "2021-02-17 05:41:10"
  }
  ```

  ### 5.The total number of masks and dollar value of transactions within a date range.
  [GET] /GetMasksAndDollar
  - startTime: 開始時間，格式yyyy-MM-dd HH:mm:ss
  - endTime: 結束時間，格式yyyy-MM-dd HH:mm:ss
  ```
  {
      "startTime":"2021-01-17 05:41:10",
      "endTime":"2021-02-17 05:41:10",    
  }
  ```

  ### 6.Search for pharmacies or masks by name, ranked by relevance to the search term.
  [GET] /SearchPharmaciesOrMasks
  -    name，藥局或口罩名稱
  ```
  {   
      "name" : "DFW Wellness"
  }
  ```

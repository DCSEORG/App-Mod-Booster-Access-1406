# Dial Direct Car Insurance â Demo App Notes

## How to Use
1. Open `index.html` in a web browser (no server needed â fully standalone)
2. Work through each step using the test data below

## Test Data to Navigate the Full Demo

### Step 1 â Your Details
- Title: **Mr**
- First name: **John**
- Surname: **Smith**
- Date of birth: **15/03/1985**

### Step 2 â Your Car (Registration Lookup)
- Registration: **D500 NLE**
- Confirm vehicle: **Yes** (2019 Volkswagen Golf 1.5 TSI SE)
- Bought: **06/2022**
- Registered keeper: **Yes**
- Owner: **You (the proposer)**
- Use: **Social and commuting**
- Annual mileage: **10,001â12,000**
- Overnight: **Driveway at home**
- Modified: **No**
- Licence type: **Full UK**
- Years held: **20 or more years**
- NCB: **9 or more years**
- Claims last 5 years: **No**
- Convictions: **No**

### Step 3 â Your Cover
- Cover type: **Comprehensive** (pre-selected)
- Start date: Tomorrowâs date
- Voluntary excess: **£250**
- Payment: **Monthly**
- Additional drivers: **No**

### Step 4 â Quote
- Your quote will be displayed with a monthly and annual price
- Click âBuy nowâ or âPay annuallyâ to complete (demo only â no actual purchase)

## Pages
| File | Description |
|------|-------------|
| index.html | Your Details â step 1 |
| your-car.html | Your Car â step 2 |
| your-cover.html | Your Cover â step 3 |
| quote.html | Your Quote â step 4 |

## Technical Notes
- Fully standalone â no internet connection required after initial load
- Uses sessionStorage to pass data between pages
- Compatible with all modern browsers (Chrome, Firefox, Safari, Edge)
- Responsive â works on mobile, tablet and desktop

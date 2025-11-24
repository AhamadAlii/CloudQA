# CloudQA Automation Assignment

This project contains automated tests written in C# using Selenium WebDriver and NUnit.  
The tests were created as part of the CloudQA Developer Internship assignment.

## Task Requirements
1. Automate any three fields on the Automation Practice Form page.  
2. Design the tests in a way that they continue working even if the elements’ position  
   or their HTML attributes change.

Both requirements have been successfully implemented.

## Automated Fields
The following three fields were selected and automated:
- First Name (text input)
- Last Name (text input)
- Hobby – Reading (checkbox)

These fields were chosen because they appear consistently across all forms on the page  
and allow clear demonstration of text input + checkbox interaction.

## Stable Locator Strategy
To ensure the tests continue working even if HTML structure changes:
- Locators are based on **nearby label text**, not IDs or class names.
- XPath uses **normalize-space()** and **label-to-input relationship**.
- No element is selected using fragile attributes like index or auto-generated IDs.
- Each interaction uses fallback logic to find the correct field even if DOM changes.

This makes the tests resilient against:
- Reordered form elements
- Dynamic IDs
- Additional wrapping elements
- Attribute changes

## Technologies Used
- C#
- Selenium WebDriver
- NUnit
- ChromeDriver

## How to Run
1. Install .NET SDK  
2. Clone the repository  
3. Restore dependencies:

```bash
dotnet restore
```

4. Run tests:

```bash
dotnet test
```

## Project Structure
```
CloudQATests/
│   CloudQATests.csproj
│   CloudQATests.sln
│   UnitTest1.cs
│   .gitignore
```

## Author
Ahmad Ali  

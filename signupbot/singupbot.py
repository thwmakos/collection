#
# ~thwmakos~
#
# 22/9/2022
#

# run this every 5 seconds
# using a systemd service

# needs updates when signupgenius changes their layout

import datetime
from datetime import date
from time import sleep

from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC

def signup_person(firstname, lastname, email):
    try:
        options = webdriver.firefox.options.Options()
        options.headless = True
        driver = webdriver.Firefox(options=options)
    except:
        driver.close()
        exit()

    # move to signup page, replace as appropriate
    #driver.get('https://www.signupgenius.com/go/10c0d4facaf23a7fdcf8-semester')
    driver.get('https://www.signupgenius.com/go/10c0f44a9ab2aa6fcc16-test')
    wait = WebDriverWait(driver, 15)
    agree_button = wait.until(EC.presence_of_element_located((By.XPATH, '/html/body/div[1]/div/div/div/div[2]/div/button[2]')))
    agree_button.click()
    gotit_button = wait.until(EC.presence_of_element_located((By.XPATH, "//*[contains(@class, 'sug-notice--privacy-button')]")))
    gotit_button.click()
    driver.execute_script("window.scrollTo(0, document.body.scrollHeight)")

    try:
        checkboxes = driver.find_elements(By.XPATH, "//*[contains(@id, 'checkbox')]")
        submit_button = driver.find_element(By.XPATH, "//*[contains(@class,'giantsubmitbutton')]")
    except:
        # full signup, cannot find checkboxes or submit button
        print("full signup")
        driver.close()
        exit()

    # click last checkbox
    checkboxes[-1].click()
    submit_button.click()

    sleep(2)

    name_box = driver.find_element(By.ID, "firstname")
    name_box.send_keys(firstname)
    last_box = driver.find_element(By.ID, "lastname")
    last_box.send_keys(lastname)
    mail_box = driver.find_element(By.ID, "email")
    mail_box.send_keys(email)

    # wait a bit...
    sleep(3)

    signup_button = driver.find_element(By.NAME, "btnSignUp")
    signup_button.click()

    print("found")

    driver.close()


#
# first check if it's day and time to register
#

# Tuesday, Thursday, Saturday
registration_days = [1, 3, 4, 5]
# one hour behind to account for server time
# may need to change DST!
registration_times = [[18, 30], [18, 0], [9, 0]]

#login_url  = 'https://www.signupgenius.com/register'
#singup_url = 'https://www.signupgenius.com/go/10c0d4facaf23a7fdcf8-semester'
#email      = "tomzach3@gmail.com"
#passwd     = "C7ck6Rbx$Xzua5X"

now = datetime.datetime.now()

if not now.weekday() in registration_days:
    exit()

hour, minute = registration_times[registration_days.index(now.weekday())]

if not(now.hour == hour and now.minute == minute and now.second <= 10):
    print("not time")
    #exit()

signup_person("test-name", "test-surname", "test1@gmail.com")
signup_person("test-name-2", "test-surname-2", "test2@gmail.com")

# login first
#driver.get('https://www.signupgenius.com/register')

# click privacy button
#wait = WebDriverWait(driver, 15)
#agree_button = wait.until(EC.presence_of_element_located((By.XPATH, '/html/body/div[1]/div/div/div/div[2]/div/button[2]')))
#agree_button.click()

# login
#username_box = driver.find_element(By.ID, "email")
#username_box.send_keys(email)

#password_box = driver.find_element(By.ID, "pword")
#password_box.send_keys(passwd)

#login_button = driver.find_element(By.ID, "loginBtnId")
#login_button.click()
#driver.implicitly_wait(5)

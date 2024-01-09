import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css'],
})
export class HomeComponent {
  constructor(public accountService: AccountService, private router: Router) {}

  navigateToregister() {
    this.router.navigateByUrl('/register');
  }
}

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, of } from 'rxjs';
import { Member } from 'src/app/_models/member';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root',
})
export class MembersService {
  baseUrlUsers = environment.apiUrl + '/users';
  members: Member[] = [];

  constructor(private http: HttpClient) {}

  getMembers() {
    if (this.members.length > 0) return of(this.members);
    return this.http.get<Member[]>(this.baseUrlUsers).pipe(
      map((members) => {
        this.members = members;
        return members;
      })
    );
  }

  getMember(username: string) {
    const member = this.members.find((x) => x.userName === username);
    if (member) return of(member);
    return this.http.get<Member>(`${this.baseUrlUsers}/${username}`);
  }

  updateMember(member: Member) {
    return this.http.put(this.baseUrlUsers, member).pipe(
      map(() => {
        const index = this.members.indexOf(member);
        this.members[index] = { ...this.members[index], ...member };
      })
    );
  }
}
